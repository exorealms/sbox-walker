using Sandbox.Diagnostics;

namespace Sandbox.Weapons
{
    public class FlashlightWeapon : BaseWeapon
    {
        [Property]
        public SpotLight SpotLight { get; set; }

        protected override void OnDisabled()
        {
            TurnOff();
            base.OnDisabled();
        }

        protected override void OnEnabled()
        {
            TurnOn();
            base.OnEnabled();
        }

        protected override void OnUpdate()
        {
            if (Input.Pressed("flashlight"))
            {
                Toggle();
                Log.Info($"Flashlight is on: {SpotLight.Enabled}");
            }

            base.OnUpdate();
        }

        private void Toggle()
        {
            SpotLight.Enabled = !SpotLight.Enabled;
            SetIcon();
        }

        private void TurnOn()
        {
            SpotLight.Enabled = true;
            SetIcon();
        }

        private void TurnOff()
        {
            SpotLight.Enabled = false;
            SetIcon();
        }

        private void SetIcon()
        {
            DisplayIcon = SpotLight.Enabled ? "flashlight_on" : "flashlight_off";
        }
    }
}
