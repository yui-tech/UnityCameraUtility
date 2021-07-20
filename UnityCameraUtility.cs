using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityCameraUtility
{
    /// <summary>
    /// ビュー行列の作成
    /// </summary>
    ///
    /// <url> https://forum.unity.com/threads/reproducing-cameras-worldtocameramatrix.365645/ </url>
    /// 
    /// <param name="position"> Cameraの座標 </param>
    /// <param name="rotation"> Cameraの回転 </param>
    public static Matrix4x4 WorldToCameraMatrix(Vector3 position, Quaternion rotation)
    {
        return (Matrix4x4.TRS(position, rotation, new Vector3(1f, 1f, -1f))).inverse;
    }

    /// <summary>
    /// プロジェクション行列の作成
    /// </summary>
    ///
    /// <param name="fov"> Field Of View </param>
    /// <param name="width"> 画面の横幅 </param>
    /// <param name="height"> 画面の縦幅 </param>
    /// <param name="nearClipPlane"> ニアクリッププレーン </param>
    /// <param name="farClipPlane"> ファークリッププレーン </param>
    public static Matrix4x4 ProjectionMatrix(float fov, float width, float height, float nearClipPlane, float farClipPlane)
    {
        return Matrix4x4.Perspective(fov, UnityCameraUtility.GetAspect(width, height), nearClipPlane, farClipPlane);
    }

    /// <summary>
    /// ビューポート行列の作成
    /// </summary>
    ///
    /// <url> https://edom18.hateblo.jp/entry/2019/01/06/121013 </url>
    ///
    /// <param name="width"> 画面の横幅 </param>
    /// <param name="height"> 画面の縦幅 </param>
    /// <param name="nearClipPlane"> ニアクリッププレーン </param>
    /// <param name="farClipPlane"> ファークリッププレーン </param>
    public static Matrix4x4 ViewportMatrix(float width, float height, float nearClipPlane, float farClipPlane)
    {
        Matrix4x4 mat = Matrix4x4.identity;
        mat.m00 = width * 0.5f;
        mat.m03 = width * 0.5f;
        mat.m11 = height * 0.5f;
        mat.m13 = height * 0.5f;
        mat.m22 = (farClipPlane - nearClipPlane) * 0.5f;
        mat.m23 = (farClipPlane + nearClipPlane) * 0.5f;
        return mat;
    }

    /// <summary>
    /// Screen座標からWorld座標への変換
    /// </summary>
    ///
    /// <url> https://edom18.hateblo.jp/entry/2019/01/06/121013 </url>
    /// <url> https://forum.unity.com/threads/reproducing-cameras-worldtocameramatrix.365645/ </url>
    ///
    /// <param name="screenPosition"> Screen座標 </param>
    /// <param name="nearClipPlane"> ニアクリッププレーン </param>
    /// <param name="farClipPlane"> ファークリッププレーン </param>
    /// <param name="position"> Cameraの座標 </param>
    /// <param name="rotation"> Cameraの回転 </param>
    /// <param name="fov"> Field Of View </param>
    /// <param name="width"> 画面の横幅 </param>
    /// <param name="height"> 画面の縦幅 </param>
    /// <param name="distance"> カメラからの距離 </param>
    ///
    /// <returns> 変換されたWorld座標 </returns>
    public static Vector3 ScreenToWorldPoint(Vector2 screenPosition, float nearClipPlane, float farClipPlane, Vector3 position, Quaternion rotation, float fov, float width, float height, float distance)
    {
        var customNearClipPlane = nearClipPlane + distance;
        Matrix4x4 viewMatrixInverse = WorldToCameraMatrix(position, rotation).inverse;
        Matrix4x4 projectionMatrixInverse = ProjectionMatrix(fov, width, height, customNearClipPlane, farClipPlane).inverse;
        Matrix4x4 viewportMatrixInverse = ViewportMatrix(width, height, customNearClipPlane, farClipPlane).inverse;

        Matrix4x4 matrix = viewMatrixInverse * projectionMatrixInverse * viewportMatrixInverse;

        Vector3 worldPosition = new Vector3(screenPosition.x, screenPosition.y, customNearClipPlane);

        float x = worldPosition.x * matrix.m00 + worldPosition.y * matrix.m01 + worldPosition.z * matrix.m02 + matrix.m03;
        float y = worldPosition.x * matrix.m10 + worldPosition.y * matrix.m11 + worldPosition.z * matrix.m12 + matrix.m13;
        float z = worldPosition.x * matrix.m20 + worldPosition.y * matrix.m21 + worldPosition.z * matrix.m22 + matrix.m23;
        float w = worldPosition.x * matrix.m30 + worldPosition.y * matrix.m31 + worldPosition.z * matrix.m32 + matrix.m33;

        if (w == 0f)
        {
            return Vector3.zero;
        }

        x /= w;
        y /= w;
        z /= w;

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// World座標からScreen座標への変換
    /// </summary>
    ///
    /// <url> https://stackoverflow.com/questions/8491247/c-opengl-convert-world-coords-to-screen2d-coords </url>
    ///
    /// <param name="worldPosition"> World座標 </param>
    /// <param name="nearClipPlane"> ニアクリッププレーン </param>
    /// <param name="farClipPlane"> ファークリッププレーン </param>
    /// <param name="position"> Cameraの座標 </param>
    /// <param name="rotation"> Cameraの回転 </param>
    /// <param name="fov"> Field Of View </param>
    /// <param name="width"> 画面の横幅 </param>
    /// <param name="height"> 画面の縦幅 </param>
    ///
    /// <returns> 変換されたWorld座標 </returns>
    public static Vector2 WorldToScreenPoint(Vector3 worldPosition, float nearClipPlane, float farClipPlane, Vector3 position, Quaternion rotation, float fov, float width, float height)
    {
        Matrix4x4 viewMatrix = WorldToCameraMatrix(position, rotation);
        Matrix4x4 projectionMatrix = ProjectionMatrix(fov, width, height, camera.nearClipPlane, camera.farClipPlane);

        Matrix4x4 matrix = projectionMatrix * viewMatrix;

        Vector4 clipSpacePos = matrix * new Vector4(worldPosition.x, worldPosition.y, worldPosition.z, 1f);

        Vector3 ndcSpacePos = clipSpacePos;

        if (clipSpacePos.w == 0f)
        {
            return Vector2.zero;
        }

        ndcSpacePos.x /= clipSpacePos.w;
        ndcSpacePos.y /= clipSpacePos.w;
        ndcSpacePos.z /= clipSpacePos.w;

        Vector2 windowSpacePos = ndcSpacePos;
        windowSpacePos.x = ((ndcSpacePos.x + 1.0f) * 0.5f) * width;
        windowSpacePos.y = ((ndcSpacePos.y + 1.0f) * 0.5f) * height;
        return windowSpacePos;
    }
    
    /// <summary>
    /// アスペクト比の取得
    /// </summary>
    ///
    /// <param name="width"> 幅 </param>
    /// <param name="height"> 高さ </param>
    public static float GetAspect(float width, float height)
    {
        return width / height;
    }

    /// <summary>
    /// 垂直視野とアスペクト比から、水平視野を計算する
    /// </summary>
    ///
    /// <param name="fov"> Field Of View </param>
    /// <param name="aspect"> アスペクト比 </param>
    public static float CalculateHorizontalFoV(float fov, float aspect)
    {
        return Mathf.Atan(Mathf.Tan(fov / 2f * Mathf.Deg2Rad) * aspect) * 2f * Mathf.Rad2Deg;
    }

    /// <summary>
    /// 水平視野とアスペクト比から、垂直視野を計算する
    /// </summary>
    ///
    /// <param name="fov"> Field Of View </param>
    /// <param name="aspect"> アスペクト比 </param>
    public static float CalculateVerticalFoV(float fov, float aspect)
    {
        return Mathf.Atan(Mathf.Tan(fov / 2f * Mathf.Deg2Rad) / aspect) * 2f * Mathf.Rad2Deg;
    }

    /// <summary>
    /// 基準の解像度とField Of Viewを元にした、現在の解像度でのField Of Viewの計算
    /// </summary>
    ///
    /// <param name="baseFov"> 基準のField Of View </param>
    /// <param name="baseFov"> 基準の解像度 </param>
    public static float CalculateFov(float baseFov, Vector2 baseResolution)
    {
        // 基準の垂直視野とアスペクト比から、基準にする水平視野を計算する
        float baseHorizontalFoV = UnityCameraUtility.CalculateHorizontalFoV(baseFov, UnityCameraUtility.GetAspect(baseResolution.x, baseResolution.y));
        float currentAspect = UnityCameraUtility.GetAspect(Screen.width, Screen.height);
        // 基準にする水平視野と現在のアスペクト比から、反映すべき垂直視野を計算する
        return UnityCameraUtility.CalculateVerticalFoV(baseHorizontalFoV, currentAspect);
    }
}
