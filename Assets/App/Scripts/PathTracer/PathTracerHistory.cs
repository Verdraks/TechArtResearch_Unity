using UnityEngine;
using UnityEngine.Rendering;

public class PathTracerHistory : CameraHistoryItem
{
    private Hash128 m_DescKey;
    private int m_Id;

    public RTHandle CurrentFrame => GetCurrentFrameRT(m_Id);
    public RTHandle PreviousFrame => GetPreviousFrameRT(m_Id);

    public override void OnCreate(BufferedRTHandleSystem owner, uint typeId)
    {
        base.OnCreate(owner, typeId);
        m_Id = MakeId(0);
    }

    public override void Reset()
    {
        ReleaseHistoryFrameRT(m_Id);
    }

    public void Update(RenderTextureDescriptor descriptor)
    {
        if (m_DescKey != Hash128.Compute(ref descriptor))
        {
            ReleaseHistoryFrameRT(m_Id);
        }

        if (CurrentFrame == null)
        {
            AllocHistoryFrameRT(m_Id, 2, ref descriptor, "");
            m_DescKey = Hash128.Compute(ref descriptor);
        }
    }
	
}