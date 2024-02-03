#ifndef OVR_FREEPIE_6DOF_H
#define OVR_FREEPIE_6DOF_H


typedef struct ovr_freepie_6dof
{
	float left[3];
	float up[3];
	float forward[3];
	float position[3];
} ovr_freepie_6dof;

typedef struct ovr_freepie_data {

	ovr_freepie_6dof head;
	ovr_freepie_6dof leftHand;
	ovr_freepie_6dof rightHand;

	float LeftTrigger;
	float RightTrigger;

	float LeftGrip;
	float RightGrip;

	float LeftStickAxes[2];
	float RightStickAxes[2];

	unsigned long long LeftButtonsPressed;
	unsigned long long LeftButtonsTouched;

	unsigned long long RightButtonsPressed;
	unsigned long long RightButtonsTouched;

	unsigned int StatusHead;
	unsigned int StatusLeftHand;
	unsigned int StatusRightHand;
	unsigned int HmdMounted;

} ovr_freepie_data;

int ovr_freepie_init();
int ovr_freepie_read(ovr_freepie_data* output);
int ovr_freepie_destroy();
int ovr_freepie_reset_orientation();
int ovr_freepie_trigger_haptic_pulse(unsigned int controllerIndex, unsigned int durationMicroSec, float frequency, float amplitude);

#endif