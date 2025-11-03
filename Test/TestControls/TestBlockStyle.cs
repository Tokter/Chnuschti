using Chnuschti;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.TestControls;

public class TestBlockStyle : Style
{
    public TestBlockStyle()
    {
        Renderer = new TestBlockRenderer();
    }
}

public class TestBlockRenderState : RenderState
{
}

public class TestBlockRenderer : Renderer<TestBlock, TestBlockRenderState>
{
    public override void OnRender(SKCanvas canvas, TestBlock element, TestBlockRenderState resource, double deltaTime)
    {
    }

    public override SKSize OnMeasure(TestBlock element, TestBlockRenderState resource, SKSize availableContent)
    {
        return new SKSize(element.ContentWidth, element.ContentHeight);
    }

    public override void OnInitialize(TestBlock element, TestBlockRenderState resource)
    {
    }

    public override void OnUpdateRenderState(TestBlock element, TestBlockRenderState resource)
    {
    }
}