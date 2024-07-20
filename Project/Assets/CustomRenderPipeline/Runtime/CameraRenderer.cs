using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    ScriptableRenderContext context;
    Camera camera;
    CullingResults cullingResults;

    const string bufferName = "Render Camera";
    CommandBuffer buffer = new CommandBuffer {
        name = bufferName
    };

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull()) {
            return;
        }
        SetUp();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }

    void SetUp()
    {
        context.SetupCameraProperties(camera);
        buffer.ClearRenderTarget(true, true, Color.clear);
        buffer.BeginSample(bufferName);
        ExecuteBuffer();
        // Setup unity_MatrixVP in shader (view_projection matrix)
        // view matrix: camera's position and orientation, with projection matrix: perspective or orthographic
    }

    void Submit()
    {
        buffer.EndSample(bufferName);
        ExecuteBuffer();
        context.Submit();
    }

    void DrawVisibleGeometry()
    {
        // Opaque
        var sortingSettings = new SortingSettings(camera) { 
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(
            unlitShaderTagId, sortingSettings 
        );
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        // Sky box
        context.DrawSkybox(camera);

        // Transparent
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
    }

    void ExecuteBuffer()
    {
        // The context delays the actual rendering until we submit it before execute
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }

}
