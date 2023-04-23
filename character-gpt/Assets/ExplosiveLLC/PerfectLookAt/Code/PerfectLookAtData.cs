using UnityEngine;

public enum FwdDirection
{
    X_AXIS = 0,
    Y_AXIS = 1,
    Z_AXIS = 2,
    MINUS_X_AXIS = 3,
    MINUS_Y_AXIS = 4,
    MINUS_Z_AXIS = 5
}

[System.Serializable]
public class PerfectLookAtData
{
    private Quaternion m_DefaultRotation;
    public Transform m_Bone;
    public float m_RotationLimit = 90.0f;
    public float m_RotateAroundUpVectorWeight = 0.0f;
    public FwdDirection m_ForwardAxis;
    public FwdDirection m_ParentBoneForwardAxis;
    public bool m_ResetToDefaultRotation = false;
    public PerfecLookAtLinkedBones[] m_LinkedBones;

    public void SetDefaultRotation( Quaternion rot )
    { m_DefaultRotation = rot; }

    public Quaternion GetDefaultRotation()
    { return m_DefaultRotation; }

    public void CheckJointRotation()
    {
        if ( m_RotationLimit < Mathf.Epsilon ) {
            Debug.LogWarning( "Joint limit is zero or negative. No rotation will take effect" );
        }
    }
}

[System.Serializable]
public class PerfecLookAtLinkedBones
{
    private Quaternion m_DefaultRotation;
    private Quaternion m_LastFrameRotation;
    public Transform m_Bone;
    public bool m_ResetToDefaultRotation = false;

    public void SetDefaultRotation( Quaternion rot )
    { m_DefaultRotation = rot; }

    public Quaternion GetDefaultRotation()
    { return m_DefaultRotation; }

    public void SetLastFrameRotation( Quaternion rotation )
    { m_LastFrameRotation = rotation; }

    public Quaternion GetLastFrameRotation()
    { return m_LastFrameRotation; }
}