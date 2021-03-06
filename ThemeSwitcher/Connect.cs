using System;
using System.Windows.Forms;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using ThemeSwitcher.Forms;
using ThemeSwitcher.Model;
using ThemeSwitcher.Services;

namespace ThemeSwitcher
{
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
	    /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
	    /// <param name='application'>Root object of the host application.</param>
	    /// <param name='connectMode'>Describes how the Add-in is being loaded.</param>
	    /// <param name='addInInst'>Object representing this Add-in.</param>
	    /// <param name="custom"></param>
	    /// <seealso class='IDTExtensibility2' />
	    public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
	    {
            try
            {
                Context.Application = (DTE2) application;
                Context.AddIn = (AddIn) addInInst;

                if (connectMode != ext_ConnectMode.ext_cm_UISetup)
                    return;

                var contextGuids = new object[] {};
                var commands = (Commands2) Context.Application.Commands;

                //Place the command on the tools menu.
                //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
                var menuBarCommandBar = ((CommandBars) Context.Application.CommandBars)["MenuBar"];

                //Find the Tools command bar on the MenuBar command bar:
                var toolsControl = menuBarCommandBar.Controls[GetMenuName("Tools")];
                var toolsPopup = (CommandBarPopup) toolsControl;

                try
                {
                    var command = commands.AddNamedCommand2(Context.AddIn,
                                                            "Config",
                                                            "ThemeSwitcher configuration",
                                                            "Show the ThemeSwitcher configuration dialog",
                                                            false,
                                                            0,
                                                            ref contextGuids,
                                                            (int) vsCommandStatus.vsCommandStatusSupported +
                                                            (int) vsCommandStatus.vsCommandStatusEnabled,
                                                            (int) vsCommandStyle.vsCommandStyleText,
                                                            vsCommandControlType.vsCommandControlTypeButton);

                    if ((command != null) && (toolsPopup != null))
                        command.AddControl(toolsPopup.CommandBar, 1);
                }
                catch (ArgumentException) {}

                try
                {
                    var command = commands.AddNamedCommand2(Context.AddIn,
                                                            "Next",
                                                            "Next theme",
                                                            "Switch to the next configured theme",
                                                            false,
                                                            0,
                                                            ref contextGuids,
                                                            (int) vsCommandStatus.vsCommandStatusSupported +
                                                            (int) vsCommandStatus.vsCommandStatusEnabled,
                                                            (int) vsCommandStyle.vsCommandStyleText,
                                                            vsCommandControlType.vsCommandControlTypeButton);

                    command.Bindings = "Global::Ctrl+#";
                }
                catch (ArgumentException) {}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ThemeSwitcher");
            }
		}

	    /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param name='commandName'>The name of the command to determine state for.</param>
		/// <param name='neededText'>Text that is needed for the command.</param>
		/// <param name='status'>The state of the command in the user interface.</param>
		/// <param name='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
	    {
	        if (neededText != vsCommandStatusTextWanted.vsCommandStatusTextWantedNone) return;

	        switch (commandName)
	        {
	            case "ThemeSwitcher.Connect.Config":
	            case "ThemeSwitcher.Connect.Next":
	                status = vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
	                return;
	        }
	    }

	    /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
		/// <param name='commandName'>The name of the command to execute.</param>
		/// <param name='executeOption'>Describes how the command should be run.</param>
		/// <param name='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param name='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param name='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
			handled = false;
		    if (executeOption != vsCommandExecOption.vsCommandExecOptionDoDefault) return;

		    switch (commandName)
		    {
		        case "ThemeSwitcher.Connect.Config":
		            new ConfigForm().ShowDialog();
		            handled = true;
		            return;
		        case "ThemeSwitcher.Connect.Next":
		            ThemeCycler.LoadNextTheme();
		            handled = true;
		            return;
		    }
        }

        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            
        }
        public void OnAddInsUpdate(ref Array custom) { }
        public void OnStartupComplete(ref Array custom) { }
        public void OnBeginShutdown(ref Array custom) { }

        private static string GetMenuName(string englishName)
        {
            try
            {
                string resourceName;
                var resourceManager = new ResourceManager("ThemeSwitcher.CommandBar", Assembly.GetExecutingAssembly());
                var cultureInfo = new CultureInfo(Context.Application.LocaleID);

                if (cultureInfo.TwoLetterISOLanguageName == "zh")
                {
                    var parentCultureInfo = cultureInfo.Parent;
                    resourceName = String.Concat(parentCultureInfo.Name, englishName);
                }
                else
                    resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, englishName);

                return resourceManager.GetString(resourceName);
            }
            catch
            {
                //We tried to find a localized version of the word Tools, but one was not found.
                //  Default to the en-US word, which may work for the current culture.
                return englishName;
            }
        }
	}
}