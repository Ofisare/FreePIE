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

	float LTrigger;
	float RTrigger;

	float LGrip;
	float RGrip;

	float Lstick[2];
	float Rstick[2];

	float A;
	float B;
	float X;
	float Y;
	float LThumb;
	float RThumb;
	float Menu;
	float Home;

	unsigned int statusHead;
	unsigned int statusLeftHand;
	unsigned int statusRightHand;
	unsigned int HmdMounted;

} ovr_freepie_data;

int ovr_freepie_init();
int ovr_freepie_read(ovr_freepie_data* output);
int ovr_freepie_destroy();
int ovr_freepie_reset_orientation();
int ovr_freepie_trigger_haptic_pulse(unsigned int controllerIndex, unsigned int durationMicroSec, float frequency, float amplitude);

#endif