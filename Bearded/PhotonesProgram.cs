﻿using System;
using System.Globalization;
using System.Threading;
using amulware.Graphics;
using Bearded.Photones.GameUI;
using Bearded.Utilities.Input;
using Bearded.Photones.Rendering;
using Bearded.Photones.Screens;
using Bearded.Utilities.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace Bearded.Photones {
    class PhotonesProgram : Program {
        static void Main(string[] args) {
            using (Toolkit.Init(new ToolkitOptions() { Backend = PlatformBackend.PreferNative })) {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

                var logger = new Logger();

                logger.Info.Log("");
                logger.Info.Log("Creating game");
                var game = new PhotonesProgram(logger);

                logger.Info.Log("Running game");
                game.Run(60);

                logger.Info.Log("Safely exited game");
            }
        }

        // NOTE: This is probably a sign that I don't use the ViewportSize class like I should.
        public const float WIDTH = 1280;
        public const float HEIGHT = 720;

        public const int MAJOR = 0;
        public const int MINOR = 0;

        private readonly Logger logger;

        private InputManager inputManager;
        private RenderContext renderContext;
        private ScreenManager screenManager;

        public PhotonesProgram(Logger logger)
            : base((int)WIDTH, (int)HEIGHT, GraphicsMode.Default, "photones",
                GameWindowFlags.Default, DisplayDevice.Default, MAJOR, MINOR, GraphicsContextFlags.Default) {
            Console.WriteLine(DisplayDevice.Default.ToString());
            Console.WriteLine(OpenTK.Graphics.OpenGL.GL.GetString(OpenTK.Graphics.OpenGL.StringName.Renderer));
            Console.WriteLine(OpenTK.Graphics.OpenGL.GL.GetString(OpenTK.Graphics.OpenGL.StringName.Version));
            this.logger = logger;
        }

        protected override void OnLoad(EventArgs e) {
            renderContext = new RenderContext();

            inputManager = new InputManager(this);

            screenManager = new ScreenManager(inputManager);
            screenManager.AddScreenLayerOnTop(new GameScreen(screenManager, renderContext.Geometries));
            screenManager.AddScreenLayerOnTop(new HudScreen(screenManager, renderContext.Geometries));

            KeyPress += (sender, args) => screenManager.RegisterPressedCharacter(args.KeyChar);

            OnResize(EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e) {
            screenManager.OnResize(new ViewportSize(Width, Height));
        }

        protected override void OnUpdateUIThread() {
            inputManager.ProcessEventsAsync();
        }

        protected override void OnUpdate(UpdateEventArgs e) {
            inputManager.Update(Focused);

            if (inputManager.IsKeyPressed(Key.AltLeft) && inputManager.IsKeyPressed(Key.F4)) {
                Close();
            }

            screenManager.Update(e);
        }

        protected override void OnRender(UpdateEventArgs e) {
            renderContext.Compositor.PrepareForFrame();
            screenManager.Render(renderContext);
            renderContext.Compositor.FinalizeFrame();

            SwapBuffers();
        }
    }
}