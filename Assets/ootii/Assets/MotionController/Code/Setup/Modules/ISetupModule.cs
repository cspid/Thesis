using com.ootii.Actors.AnimationControllers;

namespace com.ootii.Setup
{
    public interface ISetupModule
    {
#if UNITY_EDITOR
        float Priority { get; set; }
        int Category { get; set; }
        bool IsValid { get; }

        void Initialize(bool rUseDefaults);
        void BeginSetup(MotionController rMotionController, bool rIsPlayer);        
#endif
    }
}

