using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer
{
    ScriptableRenderContext context;
    Camera camera;

    const string bufferName = "Render Camera";
    CommandBuffer buffer = new CommandBuffer {
        name = bufferName
    };

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        SetUp();
        DrawVisibleGeometry();
        Submit();
    }

    void SetUp()
    {
        buffer.BeginSample(bufferName);
        ExecuteBuffer();
        // Setup unity_MatrixVP in shader (view_projection matrix)
        // view matrix: camera's position and orientation, with projection matrix: perspective or orthographic
        context.SetupCameraProperties(camera);
    }

    void Submit()
    {
        buffer.EndSample(bufferName);
        ExecuteBuffer();
        context.Submit();
    }

    void DrawVisibleGeometry()
    {
        context.DrawSkybox(camera);
    }

    void ExecuteBuffer()
    { 
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }


}
