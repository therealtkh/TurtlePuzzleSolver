
namespace Turtles
{
    partial class Turtles
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
            this.tb_1 = new System.Windows.Forms.TextBox();
            this.pb_1 = new System.Windows.Forms.PictureBox();
            this.btn_start = new System.Windows.Forms.Button();
            this.btn_stop = new System.Windows.Forms.Button();
            this.btn_solutions = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pb_1)).BeginInit();
            this.SuspendLayout();
            // 
            // tb_1
            // 
            this.tb_1.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tb_1.Location = new System.Drawing.Point(619, 41);
            this.tb_1.Multiline = true;
            this.tb_1.Name = "tb_1";
            this.tb_1.ReadOnly = true;
            this.tb_1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tb_1.Size = new System.Drawing.Size(237, 504);
            this.tb_1.TabIndex = 3;
            // 
            // pb_1
            // 
            this.pb_1.Location = new System.Drawing.Point(12, 12);
            this.pb_1.Name = "pb_1";
            this.pb_1.Size = new System.Drawing.Size(593, 533);
            this.pb_1.TabIndex = 1;
            this.pb_1.TabStop = false;
            this.pb_1.Click += new System.EventHandler(this.pb_1_Click);
            // 
            // btn_start
            // 
            this.btn_start.Location = new System.Drawing.Point(619, 12);
            this.btn_start.Name = "btn_start";
            this.btn_start.Size = new System.Drawing.Size(56, 23);
            this.btn_start.TabIndex = 1;
            this.btn_start.Text = "Start";
            this.btn_start.UseVisualStyleBackColor = true;
            this.btn_start.Click += new System.EventHandler(this.btn_start_Click);
            // 
            // btn_stop
            // 
            this.btn_stop.Enabled = false;
            this.btn_stop.Location = new System.Drawing.Point(681, 12);
            this.btn_stop.Name = "btn_stop";
            this.btn_stop.Size = new System.Drawing.Size(56, 23);
            this.btn_stop.TabIndex = 2;
            this.btn_stop.Text = "Stop";
            this.btn_stop.UseVisualStyleBackColor = true;
            this.btn_stop.Click += new System.EventHandler(this.btn_stop_Click);
            // 
            // btn_solutions
            // 
            this.btn_solutions.Enabled = false;
            this.btn_solutions.Location = new System.Drawing.Point(743, 12);
            this.btn_solutions.Name = "btn_solutions";
            this.btn_solutions.Size = new System.Drawing.Size(113, 23);
            this.btn_solutions.TabIndex = 4;
            this.btn_solutions.Text = "Solutions";
            this.btn_solutions.UseVisualStyleBackColor = true;
            this.btn_solutions.Click += new System.EventHandler(this.btn_solutions_Click);
            // 
            // Turtles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(868, 557);
            this.Controls.Add(this.btn_solutions);
            this.Controls.Add(this.btn_stop);
            this.Controls.Add(this.btn_start);
            this.Controls.Add(this.pb_1);
            this.Controls.Add(this.tb_1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Turtles";
            this.Text = "Turtle Puzzle Solver";
            ((System.ComponentModel.ISupportInitialize)(this.pb_1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tb_1;
        private System.Windows.Forms.PictureBox pb_1;
        private System.Windows.Forms.Button btn_start;
        private System.Windows.Forms.Button btn_stop;
        private System.Windows.Forms.Button btn_solutions;
    }
}

