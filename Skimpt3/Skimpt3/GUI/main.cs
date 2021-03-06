﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Skimpt3.classes;

namespace Skimpt3.GUI {
    public partial class main : Form {
        private enum mode {
            FullScreen,
            CameraMode,
            HighlightMode,
        };

        private skKeyboardHook KeyboardHookInstance;
        private mode CurrentMode = mode.FullScreen;
        private Rectangle WindowFrameToCapture;
        private bool RunningInMode;
        private bool instanceCreated;
        private WindowLayer mc;
        public main() {
            InitializeComponent();
            this.Hide();
            //The following sets up a hook as soon as the program is launched.
            KeyboardHookInstance = new skKeyboardHook();
            KeyboardHookInstance.KeyIntercepted += new KeyboardEventHandler(KeyboardHookInstance_KeyIntercepted);
        }

        private void KeyboardHookInstance_KeyIntercepted(KeyboardHookEventArgs keyboardEvents) {
            skImage myImage = null;
            Bitmap i;

            if (keyboardEvents.PressedKey == Keys.PrintScreen) {
                switch (CurrentMode) {
                    case mode.FullScreen:
                        i = skImageCapture.GetDesktopWindowCaptureAsBitmap();
                        myImage = new skImage(i);
                        break;
                    case mode.CameraMode:
                        this.Hide();
                        if (!instanceCreated) {
                            mc = new WindowLayer();
                            instanceCreated = true;
                            mc.Show();                        
                        } else {
                            WindowFrameToCapture = mc.GetWindowFrame();
                            i = skImageCapture.CaptureDeskTopRectangle(WindowFrameToCapture, WindowFrameToCapture.Width, WindowFrameToCapture.Height);
                            myImage = new skImage(i);
                            mc.Dispose();
                            mc = null;
                            instanceCreated = false;
                        }
                        break;
                    case mode.HighlightMode:
                        this.Hide();
                        if (!instanceCreated) {
                            mc = new WindowLayer();
                            instanceCreated = true;
                            mc.Show();
                        } else {
                            WindowFrameToCapture = mc.GetWindowFrame();
                            i = skImageCapture.GetDesktopWindowCaptureAsBitmap();
                            myImage = new skImage(i, WindowFrameToCapture);
                            mc.Dispose();
                            mc = null;
                            instanceCreated = false;
                        }
                        break;
                    default:
                        MessageBox.Show("unable to capture screen");
                        break;
                }

                if (myImage != null) {
                    Common.ShowToastForm(myImage);
                }

            }
        }

        private void main_Resize(object sender, EventArgs e) {
            if (FormWindowState.Minimized == WindowState)
                Hide();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void cameraToolStripMenuItem_Click(object sender, EventArgs e) {
            cameraToolStripMenuItem.Checked = !cameraToolStripMenuItem.Checked;
            if (cameraToolStripMenuItem.Checked) {
                highlightToolStripMenuItem.Checked = !cameraToolStripMenuItem.Checked;
                CurrentMode = mode.CameraMode;
                RunningInMode = true;
            } else {
                CurrentMode = mode.FullScreen;
                RunningInMode = false;
            }

        }

        private void highlightToolStripMenuItem_Click(object sender, EventArgs e) {
            highlightToolStripMenuItem.Checked = !highlightToolStripMenuItem.Checked;
            if (highlightToolStripMenuItem.Checked) {
                cameraToolStripMenuItem.Checked = !highlightToolStripMenuItem.Checked;
                CurrentMode = mode.HighlightMode;
                RunningInMode = true;
            } else {
                CurrentMode = mode.FullScreen;
                RunningInMode = false;
            }
        }

        private void main_Load(object sender, EventArgs e) {
            this.Hide();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e) {
            using (var i = new Options()) {
                i.ShowDialog();
            }

        }




    }
}
