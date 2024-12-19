using UnityEngine;
using Reptile;

namespace SpeedrunUtils
{
    public class MainMenuVerManager : MonoBehaviour
    {
        private BaseModule BaseModule;
        private GameObject nameVersionText;
        private VersionUIHandler versionUIHandler;
        private IGameTextLocalizer localizer;

        public void Update()
        {
            if(Core.Instance != null)
            {
                if (BaseModule == null) { BaseModule = Core.Instance.BaseModule; }
                if (localizer == null) { localizer = Core.Instance.Localizer; }

                if (BaseModule.CurrentStage == Stage.NONE)
                {
                    if (nameVersionText == null)
                    {
                        nameVersionText = FindObjectOfType<VersionUIHandler>().gameObject;
                    }

                    if (versionUIHandler == null && nameVersionText != null)
                    {
                        versionUIHandler = nameVersionText.GetComponent<VersionUIHandler>();
                    }

                    if (versionUIHandler != null && localizer != null)
                    {

                        if (Core.Instance.GameVersion.ToString() == "1.0.19975")
                        {
                            versionUIHandler.versionText.text = "<allcaps>" + this.localizer.GetUserInterfaceText("MAIN_MENU_VERSION") + ": <color=#21c400>" + ((object)Core.Instance.GameVersion).ToString() + "</color></allcaps> - " + Core.Instance.Platform.User.UserName;
                        }
                        else
                        {
                            versionUIHandler.versionText.text = "<allcaps>" + this.localizer.GetUserInterfaceText("MAIN_MENU_VERSION") + ": <color=red>" + ((object)Core.Instance.GameVersion).ToString() + "</color></allcaps> - " + Core.Instance.Platform.User.UserName;
                        }
                    }
                }
            }
        }
    }
}
