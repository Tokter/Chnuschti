namespace WindowsFormsDemo
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            skglControl = new SkiaSharp.Views.Desktop.SKGLControl();
            SuspendLayout();
            // 
            // skglControl
            // 
            skglControl.API = OpenTK.Windowing.Common.ContextAPI.OpenGL;
            skglControl.APIVersion = new Version(3, 3, 0, 0);
            skglControl.Dock = DockStyle.Fill;
            skglControl.Flags = OpenTK.Windowing.Common.ContextFlags.Default;
            skglControl.IsEventDriven = true;
            skglControl.Location = new Point(0, 0);
            skglControl.Name = "skglControl";
            skglControl.Profile = OpenTK.Windowing.Common.ContextProfile.Core;
            skglControl.SharedContext = null;
            skglControl.Size = new Size(800, 450);
            skglControl.TabIndex = 0;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(skglControl);
            Name = "MainWindow";
            Text = "Chnuschti - Demo";
            ResumeLayout(false);
        }

        #endregion

        private SkiaSharp.Views.Desktop.SKGLControl skglControl;
    }
}
