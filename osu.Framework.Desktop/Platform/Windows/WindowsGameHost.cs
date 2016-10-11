﻿// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using osu.Framework.Desktop.Input.Handlers.Keyboard;
using osu.Framework.Desktop.Input.Handlers.Mouse;
using osu.Framework.Desktop.Platform.Windows.Native;
using osu.Framework.Input.Handlers;
using OpenTK.Graphics;

namespace osu.Framework.Desktop.Platform.Windows
{
    public class WindowsGameHost : DesktopGameHost
    {
        private TimePeriod timePeriod;
        private OpenTKKeyboardHandler keyboardHandler = new OpenTKKeyboardHandler();

        internal WindowsGameHost(GraphicsContextFlags flags, string gameName)
        {
            // OnActivate / OnDeactivate may not fire, so the initial activity state may be unknown here.
            // In order to be certain we have the correct activity state we are querying the Windows API here.
            IsActive = true; // TODO

            timePeriod = new TimePeriod(1) { Active = true };

            Architecture.SetIncludePath();

            Window = new DesktopGameWindow();
            Window.WindowStateChanged += (sender, e) =>
            {
                if (Window.WindowState != OpenTK.WindowState.Minimized)
                    OnActivated(sender, e);
                else
                    OnDeactivated(sender, e);
            };

            Storage = new WindowsStorage(gameName);
            
            Window.KeyPress += (sender, e) => keyboardHandler.OnCharacterInput(e.KeyChar);

            Application.EnableVisualStyles();
        }

        public override IEnumerable<InputHandler> GetInputHandlers()
        {
            //todo: figure why opentk input handlers aren't working.
            return new InputHandler[] { new OpenTKMouseHandler(), keyboardHandler };
        }

        protected override void Dispose(bool isDisposing)
        {
            timePeriod.Dispose();
            base.Dispose(isDisposing);
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            timePeriod.Active = true;

            Execution.SetThreadExecutionState(Execution.ExecutionState.Continuous | Execution.ExecutionState.SystemRequired | Execution.ExecutionState.DisplayRequired);
            base.OnActivated(sender, args);
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            timePeriod.Active = false;

            Execution.SetThreadExecutionState(Execution.ExecutionState.Continuous);
            base.OnDeactivated(sender, args);
        }
    }
}
