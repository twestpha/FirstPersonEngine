public class SoundConstants {
    public const float MAX_REVERB_DISTANCE = 50.0f;
    public const float MAX_OCCLUDE_DISTANCE = 50.0f;

    // Represents all but one side of the reverb volume being max
    // Used to normalize physical volume and simplify calculations
    public const float MAX_REVERB_PHYSICAL_VOLUME = 173333.0f;

    // Dry Level and Room Constants
    public const float RVB_DL = 0.0f;
    public const float RVB_RM = -1000.0f;

    // Room High Frequency Curve
    public const float RVB_RM_HF_A = 3300f;
    public const float RVB_RM_HF_B = -6600f;
    public const float RVB_RM_HF_C = 0.0f;

    // Room Low Frequency Constant
    public const float RVB_RM_LF = 0.0f;

    // Decay Time Constant
    public const float RVB_DT = 1.49f;

    // Decay HF Ratio Curve
    public const float RVB_DHFR_A = 2.4199f;
    public const float RVB_DHFR_B = -2.9199f;
    public const float RVB_DHFR_C = 1.0f;

    // Reflections Level Curve
    public const float RVB_RF_LV_A = 5877.37f;
    public const float RVB_RF_LV_B = -8657.37f;
    public const float RVB_RF_LV_C = 0.0f;

    // Reflections Delay Constant
    public const float RVB_RF_DL = 0.0f;

    // Reverb Level Curve
    public const float RVB_RV_LV_A = 2667.64f;
    public const float RVB_RV_LV_B = -4593.64f;
    public const float RVB_RV_LV_C = 0.0f;

    // Reverb Delay Curve
    public const float RVB_RV_DL_MAX = 0.4096f;
    public const float RVB_RV_DL_A = -0.556571f;
    public const float RVB_RV_DL_B = 0.445256f;
    public const float RVB_RV_DL_C = 0.011f;

    // Frequency Reference Constants
    public const float RVB_HF_R = 5000.0f;
    public const float RVB_LF_R = 250.0f;

    // Diffusion Curve
    // 79 x^2 - 158 x + 100
    public const float RVB_DF_A = 79.0f;
    public const float RVB_DF_B = -158.0f;
    public const float RVB_DF_C = 100.0f;

    // Density Constant
    public const float RVB_DN = 100.0f;

    // Low Pass Filter (Occlusion) Constants
    public const float OCCLUSION_ON  = 1.0f;
    public const float OCCLUSION_OFF = 0.0f;

    public const float LPF_OCCLUDED = 2250;
    public const float LPF_CLEAR    = 22000;

    // Doppler Constants
    public const float DOPPLER_ON  = 1.0f;
    public const float DOPPLER_OFF = 0.0f;

    // Spatial Blend Constants
    public const float SPATIAL_BLEND_TWO_D = 0.0f;
    public const float SPATIAL_BLEND_THREE_D = 1.0f;
}
