namespace RackTool
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Up", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Down", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "Box1"}, 0, System.Drawing.Color.Empty, System.Drawing.Color.Empty, new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134))));
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "Box2"}, 0, System.Drawing.Color.Empty, System.Drawing.Color.Empty, new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134))));
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem(new string[] {
            "Box3"}, 0, System.Drawing.Color.Empty, System.Drawing.Color.Empty, new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134))));
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem(new string[] {
            "Box4"}, 0, System.Drawing.Color.Empty, System.Drawing.Color.Empty, new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134))));
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem(new string[] {
            "Box5"}, 0, System.Drawing.Color.Empty, System.Drawing.Color.Empty, new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134))));
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem(new string[] {
            "Box6"}, 0, System.Drawing.Color.Empty, System.Drawing.Color.Empty, new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134))));
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonHome = new System.Windows.Forms.Button();
            this.buttonTest = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.checkBoxIsLoop = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.labelSpeed2 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.labelPositionG2 = new System.Windows.Forms.Label();
            this.labelPositionG1 = new System.Windows.Forms.Label();
            this.labelPositionR = new System.Windows.Forms.Label();
            this.labelPositionZ = new System.Windows.Forms.Label();
            this.labelPositionY = new System.Windows.Forms.Label();
            this.labelPositionX = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonG1TightOrLoose = new System.Windows.Forms.Button();
            this.buttonG2TightOrLoose = new System.Windows.Forms.Button();
            this.buttonLowSpeed = new System.Windows.Forms.Button();
            this.buttonMiddleSpeed = new System.Windows.Forms.Button();
            this.buttonHighSpeed = new System.Windows.Forms.Button();
            this.trackBarSetSpeed2 = new System.Windows.Forms.TrackBar();
            this.buttonEableR = new System.Windows.Forms.Button();
            this.buttonEableZ = new System.Windows.Forms.Button();
            this.buttonEableY = new System.Windows.Forms.Button();
            this.buttonEableX2 = new System.Windows.Forms.Button();
            this.buttonEableX1 = new System.Windows.Forms.Button();
            this.buttonEableG2 = new System.Windows.Forms.Button();
            this.buttonEableG1 = new System.Windows.Forms.Button();
            this.textBoxDistanceG2 = new System.Windows.Forms.TextBox();
            this.textBoxDistanceG1 = new System.Windows.Forms.TextBox();
            this.buttonRunG2 = new System.Windows.Forms.Button();
            this.buttonRunG1 = new System.Windows.Forms.Button();
            this.buttonPositiveR = new System.Windows.Forms.Button();
            this.buttonNagetiveR = new System.Windows.Forms.Button();
            this.buttonPositiveZ = new System.Windows.Forms.Button();
            this.buttonNagetiveZ = new System.Windows.Forms.Button();
            this.buttonPositiveY = new System.Windows.Forms.Button();
            this.buttonNagetiveY = new System.Windows.Forms.Button();
            this.buttonPositiveX2 = new System.Windows.Forms.Button();
            this.buttonNagetiveX2 = new System.Windows.Forms.Button();
            this.buttonPositiveX1 = new System.Windows.Forms.Button();
            this.buttonNagetiveX1 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label5 = new System.Windows.Forms.Label();
            this.labelSpeed1 = new System.Windows.Forms.Label();
            this.trackBarSetSpeed1 = new System.Windows.Forms.TrackBar();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.buttonLoadForTeach = new System.Windows.Forms.Button();
            this.buttonReadyForPick = new System.Windows.Forms.Button();
            this.buttonCalOffset = new System.Windows.Forms.Button();
            this.buttonBin = new System.Windows.Forms.Button();
            this.buttonUnloadAndLoad = new System.Windows.Forms.Button();
            this.buttonSaveApproach = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSavePosition = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonCreateXml = new System.Windows.Forms.Button();
            this.buttonPlace = new System.Windows.Forms.Button();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.buttonPick = new System.Windows.Forms.Button();
            this.buttonUnload = new System.Windows.Forms.Button();
            this.comboBoxMovePos = new System.Windows.Forms.ComboBox();
            this.comboBox_Gripper = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.buttonForPicked = new System.Windows.Forms.Button();
            this.buttonForReady = new System.Windows.Forms.Button();
            this.buttonForInpos = new System.Windows.Forms.Button();
            this.checkBoxPickConveyorMoveForward = new System.Windows.Forms.CheckBox();
            this.buttonConveyorStart = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.richTextBoxMessage = new System.Windows.Forms.RichTextBox();
            this.listViewBox = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.buttonOK1 = new System.Windows.Forms.Button();
            this.buttonOK2 = new System.Windows.Forms.Button();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.buttonOK3 = new System.Windows.Forms.Button();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.buttonOK4 = new System.Windows.Forms.Button();
            this.comboBox4 = new System.Windows.Forms.ComboBox();
            this.buttonOK5 = new System.Windows.Forms.Button();
            this.comboBox5 = new System.Windows.Forms.ComboBox();
            this.buttonOK6 = new System.Windows.Forms.Button();
            this.comboBox6 = new System.Windows.Forms.ComboBox();
            this.buttonBoxLoad = new System.Windows.Forms.Button();
            this.buttonBoxSave = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.启用ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.禁用ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSetSpeed2)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSetSpeed1)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage7.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(92, 47);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(146, 85);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.button_Start_Click);
            // 
            // buttonHome
            // 
            this.buttonHome.Enabled = false;
            this.buttonHome.Location = new System.Drawing.Point(90, 153);
            this.buttonHome.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonHome.Name = "buttonHome";
            this.buttonHome.Size = new System.Drawing.Size(146, 28);
            this.buttonHome.TabIndex = 1;
            this.buttonHome.Text = "Home";
            this.buttonHome.UseVisualStyleBackColor = true;
            this.buttonHome.Click += new System.EventHandler(this.button_Home_Click);
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(593, 153);
            this.buttonTest.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(89, 28);
            this.buttonTest.TabIndex = 2;
            this.buttonTest.Text = "Test";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.button3_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(320, 47);
            this.buttonStop.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(146, 85);
            this.buttonStop.TabIndex = 4;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.button5_Click);
            // 
            // checkBoxIsLoop
            // 
            this.checkBoxIsLoop.AutoSize = true;
            this.checkBoxIsLoop.Checked = true;
            this.checkBoxIsLoop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxIsLoop.Location = new System.Drawing.Point(687, 160);
            this.checkBoxIsLoop.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBoxIsLoop.Name = "checkBoxIsLoop";
            this.checkBoxIsLoop.Size = new System.Drawing.Size(48, 16);
            this.checkBoxIsLoop.TabIndex = 6;
            this.checkBoxIsLoop.Text = "Loop";
            this.checkBoxIsLoop.UseVisualStyleBackColor = true;
            this.checkBoxIsLoop.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.labelSpeed2);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.labelPositionG2);
            this.groupBox1.Controls.Add(this.labelPositionG1);
            this.groupBox1.Controls.Add(this.labelPositionR);
            this.groupBox1.Controls.Add(this.labelPositionZ);
            this.groupBox1.Controls.Add(this.labelPositionY);
            this.groupBox1.Controls.Add(this.labelPositionX);
            this.groupBox1.Location = new System.Drawing.Point(475, 19);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Size = new System.Drawing.Size(114, 222);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Position";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 174);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(23, 12);
            this.label10.TabIndex = 11;
            this.label10.Text = "G2:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(4, 143);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(23, 12);
            this.label11.TabIndex = 10;
            this.label11.Text = "G1:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(4, 112);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(17, 12);
            this.label12.TabIndex = 9;
            this.label12.Text = "R:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(4, 81);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(17, 12);
            this.label13.TabIndex = 8;
            this.label13.Text = "Z:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(4, 50);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(17, 12);
            this.label14.TabIndex = 7;
            this.label14.Text = "Y:";
            // 
            // labelSpeed2
            // 
            this.labelSpeed2.AutoSize = true;
            this.labelSpeed2.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelSpeed2.Location = new System.Drawing.Point(35, 201);
            this.labelSpeed2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelSpeed2.Name = "labelSpeed2";
            this.labelSpeed2.Size = new System.Drawing.Size(29, 19);
            this.labelSpeed2.TabIndex = 37;
            this.labelSpeed2.Text = "20";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(4, 19);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(17, 12);
            this.label15.TabIndex = 6;
            this.label15.Text = "X:";
            // 
            // labelPositionG2
            // 
            this.labelPositionG2.AutoSize = true;
            this.labelPositionG2.Location = new System.Drawing.Point(25, 174);
            this.labelPositionG2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPositionG2.Name = "labelPositionG2";
            this.labelPositionG2.Size = new System.Drawing.Size(11, 12);
            this.labelPositionG2.TabIndex = 5;
            this.labelPositionG2.Text = "0";
            // 
            // labelPositionG1
            // 
            this.labelPositionG1.AutoSize = true;
            this.labelPositionG1.Location = new System.Drawing.Point(25, 143);
            this.labelPositionG1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPositionG1.Name = "labelPositionG1";
            this.labelPositionG1.Size = new System.Drawing.Size(11, 12);
            this.labelPositionG1.TabIndex = 4;
            this.labelPositionG1.Text = "0";
            // 
            // labelPositionR
            // 
            this.labelPositionR.AutoSize = true;
            this.labelPositionR.Location = new System.Drawing.Point(25, 112);
            this.labelPositionR.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPositionR.Name = "labelPositionR";
            this.labelPositionR.Size = new System.Drawing.Size(11, 12);
            this.labelPositionR.TabIndex = 3;
            this.labelPositionR.Text = "0";
            // 
            // labelPositionZ
            // 
            this.labelPositionZ.AutoSize = true;
            this.labelPositionZ.Location = new System.Drawing.Point(25, 81);
            this.labelPositionZ.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPositionZ.Name = "labelPositionZ";
            this.labelPositionZ.Size = new System.Drawing.Size(11, 12);
            this.labelPositionZ.TabIndex = 2;
            this.labelPositionZ.Text = "0";
            // 
            // labelPositionY
            // 
            this.labelPositionY.AutoSize = true;
            this.labelPositionY.Location = new System.Drawing.Point(25, 50);
            this.labelPositionY.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPositionY.Name = "labelPositionY";
            this.labelPositionY.Size = new System.Drawing.Size(11, 12);
            this.labelPositionY.TabIndex = 1;
            this.labelPositionY.Text = "0";
            // 
            // labelPositionX
            // 
            this.labelPositionX.AutoSize = true;
            this.labelPositionX.Location = new System.Drawing.Point(25, 19);
            this.labelPositionX.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelPositionX.Name = "labelPositionX";
            this.labelPositionX.Size = new System.Drawing.Size(11, 12);
            this.labelPositionX.TabIndex = 0;
            this.labelPositionX.Text = "0";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonG1TightOrLoose);
            this.groupBox2.Controls.Add(this.buttonG2TightOrLoose);
            this.groupBox2.Controls.Add(this.buttonLowSpeed);
            this.groupBox2.Controls.Add(this.buttonMiddleSpeed);
            this.groupBox2.Controls.Add(this.buttonHighSpeed);
            this.groupBox2.Controls.Add(this.trackBarSetSpeed2);
            this.groupBox2.Controls.Add(this.groupBox1);
            this.groupBox2.Controls.Add(this.buttonEableR);
            this.groupBox2.Controls.Add(this.buttonEableZ);
            this.groupBox2.Controls.Add(this.buttonEableY);
            this.groupBox2.Controls.Add(this.buttonEableX2);
            this.groupBox2.Controls.Add(this.buttonEableX1);
            this.groupBox2.Controls.Add(this.buttonEableG2);
            this.groupBox2.Controls.Add(this.buttonEableG1);
            this.groupBox2.Controls.Add(this.textBoxDistanceG2);
            this.groupBox2.Controls.Add(this.textBoxDistanceG1);
            this.groupBox2.Controls.Add(this.buttonRunG2);
            this.groupBox2.Controls.Add(this.buttonRunG1);
            this.groupBox2.Controls.Add(this.buttonPositiveR);
            this.groupBox2.Controls.Add(this.buttonNagetiveR);
            this.groupBox2.Controls.Add(this.buttonPositiveZ);
            this.groupBox2.Controls.Add(this.buttonNagetiveZ);
            this.groupBox2.Controls.Add(this.buttonPositiveY);
            this.groupBox2.Controls.Add(this.buttonNagetiveY);
            this.groupBox2.Controls.Add(this.buttonPositiveX2);
            this.groupBox2.Controls.Add(this.buttonNagetiveX2);
            this.groupBox2.Controls.Add(this.buttonPositiveX1);
            this.groupBox2.Controls.Add(this.buttonNagetiveX1);
            this.groupBox2.Location = new System.Drawing.Point(14, 14);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Size = new System.Drawing.Size(733, 253);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Manual";
            // 
            // buttonG1TightOrLoose
            // 
            this.buttonG1TightOrLoose.Location = new System.Drawing.Point(305, 191);
            this.buttonG1TightOrLoose.Name = "buttonG1TightOrLoose";
            this.buttonG1TightOrLoose.Size = new System.Drawing.Size(70, 50);
            this.buttonG1TightOrLoose.TabIndex = 16;
            this.buttonG1TightOrLoose.Text = "G1Open";
            this.buttonG1TightOrLoose.UseVisualStyleBackColor = true;
            this.buttonG1TightOrLoose.Click += new System.EventHandler(this.buttonG1TightOrLoose_Click);
            // 
            // buttonG2TightOrLoose
            // 
            this.buttonG2TightOrLoose.Location = new System.Drawing.Point(392, 191);
            this.buttonG2TightOrLoose.Name = "buttonG2TightOrLoose";
            this.buttonG2TightOrLoose.Size = new System.Drawing.Size(70, 50);
            this.buttonG2TightOrLoose.TabIndex = 17;
            this.buttonG2TightOrLoose.Text = "G2Open";
            this.buttonG2TightOrLoose.UseVisualStyleBackColor = true;
            this.buttonG2TightOrLoose.Click += new System.EventHandler(this.buttonG2TightOrLoose_Click);
            // 
            // buttonLowSpeed
            // 
            this.buttonLowSpeed.Location = new System.Drawing.Point(662, 194);
            this.buttonLowSpeed.Name = "buttonLowSpeed";
            this.buttonLowSpeed.Size = new System.Drawing.Size(55, 46);
            this.buttonLowSpeed.TabIndex = 40;
            this.buttonLowSpeed.Text = "Low";
            this.buttonLowSpeed.UseVisualStyleBackColor = true;
            this.buttonLowSpeed.Click += new System.EventHandler(this.buttonLowSpeed_Click);
            // 
            // buttonMiddleSpeed
            // 
            this.buttonMiddleSpeed.Location = new System.Drawing.Point(662, 106);
            this.buttonMiddleSpeed.Name = "buttonMiddleSpeed";
            this.buttonMiddleSpeed.Size = new System.Drawing.Size(55, 46);
            this.buttonMiddleSpeed.TabIndex = 39;
            this.buttonMiddleSpeed.Text = "Middle";
            this.buttonMiddleSpeed.UseVisualStyleBackColor = true;
            this.buttonMiddleSpeed.Click += new System.EventHandler(this.buttonMiddleSpeed_Click);
            // 
            // buttonHighSpeed
            // 
            this.buttonHighSpeed.Location = new System.Drawing.Point(662, 18);
            this.buttonHighSpeed.Name = "buttonHighSpeed";
            this.buttonHighSpeed.Size = new System.Drawing.Size(55, 46);
            this.buttonHighSpeed.TabIndex = 38;
            this.buttonHighSpeed.Text = "High";
            this.buttonHighSpeed.UseVisualStyleBackColor = true;
            this.buttonHighSpeed.Click += new System.EventHandler(this.buttonHighSpeed_Click);
            // 
            // trackBarSetSpeed2
            // 
            this.trackBarSetSpeed2.AutoSize = false;
            this.trackBarSetSpeed2.Location = new System.Drawing.Point(593, 18);
            this.trackBarSetSpeed2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.trackBarSetSpeed2.Maximum = 100;
            this.trackBarSetSpeed2.Name = "trackBarSetSpeed2";
            this.trackBarSetSpeed2.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.trackBarSetSpeed2.Size = new System.Drawing.Size(64, 223);
            this.trackBarSetSpeed2.TabIndex = 36;
            this.trackBarSetSpeed2.Value = 10;
            this.trackBarSetSpeed2.Scroll += new System.EventHandler(this.trackBarSetSpeed_Scroll);
            // 
            // buttonEableR
            // 
            this.buttonEableR.Location = new System.Drawing.Point(113, 148);
            this.buttonEableR.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonEableR.Name = "buttonEableR";
            this.buttonEableR.Size = new System.Drawing.Size(84, 31);
            this.buttonEableR.TabIndex = 35;
            this.buttonEableR.Text = "Enable";
            this.buttonEableR.UseVisualStyleBackColor = true;
            this.buttonEableR.Click += new System.EventHandler(this.buttonEableR_Click);
            // 
            // buttonEableZ
            // 
            this.buttonEableZ.Location = new System.Drawing.Point(113, 191);
            this.buttonEableZ.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonEableZ.Name = "buttonEableZ";
            this.buttonEableZ.Size = new System.Drawing.Size(84, 50);
            this.buttonEableZ.TabIndex = 34;
            this.buttonEableZ.Text = "Enable";
            this.buttonEableZ.UseVisualStyleBackColor = true;
            this.buttonEableZ.Click += new System.EventHandler(this.buttonEableZ_Click);
            // 
            // buttonEableY
            // 
            this.buttonEableY.Location = new System.Drawing.Point(113, 105);
            this.buttonEableY.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonEableY.Name = "buttonEableY";
            this.buttonEableY.Size = new System.Drawing.Size(84, 31);
            this.buttonEableY.TabIndex = 33;
            this.buttonEableY.Text = "Enable";
            this.buttonEableY.UseVisualStyleBackColor = true;
            this.buttonEableY.Click += new System.EventHandler(this.buttonEableY_Click);
            // 
            // buttonEableX2
            // 
            this.buttonEableX2.Location = new System.Drawing.Point(113, 62);
            this.buttonEableX2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonEableX2.Name = "buttonEableX2";
            this.buttonEableX2.Size = new System.Drawing.Size(84, 31);
            this.buttonEableX2.TabIndex = 32;
            this.buttonEableX2.Text = "Enable";
            this.buttonEableX2.UseVisualStyleBackColor = true;
            this.buttonEableX2.Click += new System.EventHandler(this.buttonEableX2_Click);
            // 
            // buttonEableX1
            // 
            this.buttonEableX1.Location = new System.Drawing.Point(113, 19);
            this.buttonEableX1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonEableX1.Name = "buttonEableX1";
            this.buttonEableX1.Size = new System.Drawing.Size(84, 31);
            this.buttonEableX1.TabIndex = 31;
            this.buttonEableX1.Text = "Enable";
            this.buttonEableX1.UseVisualStyleBackColor = true;
            this.buttonEableX1.Click += new System.EventHandler(this.buttonEableX1_Click);
            // 
            // buttonEableG2
            // 
            this.buttonEableG2.Location = new System.Drawing.Point(389, 18);
            this.buttonEableG2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonEableG2.Name = "buttonEableG2";
            this.buttonEableG2.Size = new System.Drawing.Size(70, 31);
            this.buttonEableG2.TabIndex = 30;
            this.buttonEableG2.Text = "Enable";
            this.buttonEableG2.UseVisualStyleBackColor = true;
            this.buttonEableG2.Click += new System.EventHandler(this.button21_Click);
            // 
            // buttonEableG1
            // 
            this.buttonEableG1.Location = new System.Drawing.Point(303, 19);
            this.buttonEableG1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonEableG1.Name = "buttonEableG1";
            this.buttonEableG1.Size = new System.Drawing.Size(70, 31);
            this.buttonEableG1.TabIndex = 29;
            this.buttonEableG1.Text = "Enable";
            this.buttonEableG1.UseVisualStyleBackColor = true;
            this.buttonEableG1.Click += new System.EventHandler(this.button22_Click);
            // 
            // textBoxDistanceG2
            // 
            this.textBoxDistanceG2.Location = new System.Drawing.Point(389, 96);
            this.textBoxDistanceG2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxDistanceG2.Name = "textBoxDistanceG2";
            this.textBoxDistanceG2.Size = new System.Drawing.Size(71, 21);
            this.textBoxDistanceG2.TabIndex = 28;
            this.textBoxDistanceG2.Text = "0";
            this.textBoxDistanceG2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxDistanceG1
            // 
            this.textBoxDistanceG1.Location = new System.Drawing.Point(303, 97);
            this.textBoxDistanceG1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBoxDistanceG1.Name = "textBoxDistanceG1";
            this.textBoxDistanceG1.Size = new System.Drawing.Size(71, 21);
            this.textBoxDistanceG1.TabIndex = 27;
            this.textBoxDistanceG1.Text = "0";
            this.textBoxDistanceG1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonRunG2
            // 
            this.buttonRunG2.Location = new System.Drawing.Point(389, 62);
            this.buttonRunG2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonRunG2.Name = "buttonRunG2";
            this.buttonRunG2.Size = new System.Drawing.Size(70, 31);
            this.buttonRunG2.TabIndex = 26;
            this.buttonRunG2.Text = "G2MoveTo";
            this.buttonRunG2.UseVisualStyleBackColor = true;
            this.buttonRunG2.Click += new System.EventHandler(this.button18_Click);
            // 
            // buttonRunG1
            // 
            this.buttonRunG1.Location = new System.Drawing.Point(303, 62);
            this.buttonRunG1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonRunG1.Name = "buttonRunG1";
            this.buttonRunG1.Size = new System.Drawing.Size(70, 31);
            this.buttonRunG1.TabIndex = 25;
            this.buttonRunG1.Text = "G1MoveTo";
            this.buttonRunG1.UseVisualStyleBackColor = true;
            this.buttonRunG1.Click += new System.EventHandler(this.button19_Click);
            // 
            // buttonPositiveR
            // 
            this.buttonPositiveR.Location = new System.Drawing.Point(205, 148);
            this.buttonPositiveR.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonPositiveR.Name = "buttonPositiveR";
            this.buttonPositiveR.Size = new System.Drawing.Size(86, 31);
            this.buttonPositiveR.TabIndex = 24;
            this.buttonPositiveR.Text = "R+";
            this.buttonPositiveR.UseVisualStyleBackColor = true;
            this.buttonPositiveR.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button16_MouseDown);
            this.buttonPositiveR.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button16_MouseUp);
            // 
            // buttonNagetiveR
            // 
            this.buttonNagetiveR.Location = new System.Drawing.Point(15, 148);
            this.buttonNagetiveR.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonNagetiveR.Name = "buttonNagetiveR";
            this.buttonNagetiveR.Size = new System.Drawing.Size(86, 31);
            this.buttonNagetiveR.TabIndex = 23;
            this.buttonNagetiveR.Text = "R-";
            this.buttonNagetiveR.UseVisualStyleBackColor = true;
            this.buttonNagetiveR.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button17_MouseDown);
            this.buttonNagetiveR.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button17_MouseUp);
            // 
            // buttonPositiveZ
            // 
            this.buttonPositiveZ.Location = new System.Drawing.Point(205, 191);
            this.buttonPositiveZ.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonPositiveZ.Name = "buttonPositiveZ";
            this.buttonPositiveZ.Size = new System.Drawing.Size(86, 50);
            this.buttonPositiveZ.TabIndex = 22;
            this.buttonPositiveZ.Text = "Z+";
            this.buttonPositiveZ.UseVisualStyleBackColor = true;
            this.buttonPositiveZ.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button12_MouseDown);
            this.buttonPositiveZ.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button12_MouseUp);
            // 
            // buttonNagetiveZ
            // 
            this.buttonNagetiveZ.Location = new System.Drawing.Point(15, 191);
            this.buttonNagetiveZ.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonNagetiveZ.Name = "buttonNagetiveZ";
            this.buttonNagetiveZ.Size = new System.Drawing.Size(86, 50);
            this.buttonNagetiveZ.TabIndex = 21;
            this.buttonNagetiveZ.Text = "Z-";
            this.buttonNagetiveZ.UseVisualStyleBackColor = true;
            this.buttonNagetiveZ.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button13_MouseDown);
            this.buttonNagetiveZ.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button13_MouseUp);
            // 
            // buttonPositiveY
            // 
            this.buttonPositiveY.Location = new System.Drawing.Point(205, 105);
            this.buttonPositiveY.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonPositiveY.Name = "buttonPositiveY";
            this.buttonPositiveY.Size = new System.Drawing.Size(86, 31);
            this.buttonPositiveY.TabIndex = 20;
            this.buttonPositiveY.Text = "Y+";
            this.buttonPositiveY.UseVisualStyleBackColor = true;
            this.buttonPositiveY.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button14_MouseDown);
            this.buttonPositiveY.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button14_MouseUp);
            // 
            // buttonNagetiveY
            // 
            this.buttonNagetiveY.Location = new System.Drawing.Point(15, 105);
            this.buttonNagetiveY.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonNagetiveY.Name = "buttonNagetiveY";
            this.buttonNagetiveY.Size = new System.Drawing.Size(86, 31);
            this.buttonNagetiveY.TabIndex = 19;
            this.buttonNagetiveY.Text = "Y-";
            this.buttonNagetiveY.UseVisualStyleBackColor = true;
            this.buttonNagetiveY.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button15_MouseDown);
            this.buttonNagetiveY.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button15_MouseUp);
            // 
            // buttonPositiveX2
            // 
            this.buttonPositiveX2.Location = new System.Drawing.Point(205, 62);
            this.buttonPositiveX2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonPositiveX2.Name = "buttonPositiveX2";
            this.buttonPositiveX2.Size = new System.Drawing.Size(86, 31);
            this.buttonPositiveX2.TabIndex = 18;
            this.buttonPositiveX2.Text = "X2+";
            this.buttonPositiveX2.UseVisualStyleBackColor = true;
            this.buttonPositiveX2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button11_MouseDown);
            this.buttonPositiveX2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button11_MouseUp);
            // 
            // buttonNagetiveX2
            // 
            this.buttonNagetiveX2.Location = new System.Drawing.Point(15, 62);
            this.buttonNagetiveX2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonNagetiveX2.Name = "buttonNagetiveX2";
            this.buttonNagetiveX2.Size = new System.Drawing.Size(86, 31);
            this.buttonNagetiveX2.TabIndex = 17;
            this.buttonNagetiveX2.Text = "X2-";
            this.buttonNagetiveX2.UseVisualStyleBackColor = true;
            this.buttonNagetiveX2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button10_MouseDown);
            this.buttonNagetiveX2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonNagetiveX2_MouseUp);
            // 
            // buttonPositiveX1
            // 
            this.buttonPositiveX1.Location = new System.Drawing.Point(205, 19);
            this.buttonPositiveX1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonPositiveX1.Name = "buttonPositiveX1";
            this.buttonPositiveX1.Size = new System.Drawing.Size(86, 31);
            this.buttonPositiveX1.TabIndex = 16;
            this.buttonPositiveX1.Text = "X1+";
            this.buttonPositiveX1.UseVisualStyleBackColor = true;
            this.buttonPositiveX1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button9_MouseDown);
            this.buttonPositiveX1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button9_MouseUp);
            // 
            // buttonNagetiveX1
            // 
            this.buttonNagetiveX1.Location = new System.Drawing.Point(15, 19);
            this.buttonNagetiveX1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonNagetiveX1.Name = "buttonNagetiveX1";
            this.buttonNagetiveX1.Size = new System.Drawing.Size(86, 31);
            this.buttonNagetiveX1.TabIndex = 15;
            this.buttonNagetiveX1.Text = "X1-";
            this.buttonNagetiveX1.UseVisualStyleBackColor = true;
            this.buttonNagetiveX1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button8_MouseDown);
            this.buttonNagetiveX1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button8_MouseUp);
            // 
            // tabControl1
            // 
            this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Controls.Add(this.tabPage7);
            this.tabControl1.ItemSize = new System.Drawing.Size(50, 100);
            this.tabControl1.Location = new System.Drawing.Point(21, 69);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(912, 486);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl1.TabIndex = 18;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.buttonHome);
            this.tabPage1.Controls.Add(this.buttonStart);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.labelSpeed1);
            this.tabPage1.Controls.Add(this.trackBarSetSpeed1);
            this.tabPage1.Controls.Add(this.buttonTest);
            this.tabPage1.Controls.Add(this.checkBoxIsLoop);
            this.tabPage1.Controls.Add(this.buttonStop);
            this.tabPage1.Location = new System.Drawing.Point(104, 4);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Size = new System.Drawing.Size(804, 478);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Main";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(5, 267);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(79, 19);
            this.label5.TabIndex = 26;
            this.label5.Text = "Speed: ";
            // 
            // labelSpeed1
            // 
            this.labelSpeed1.AutoSize = true;
            this.labelSpeed1.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelSpeed1.Location = new System.Drawing.Point(744, 267);
            this.labelSpeed1.Name = "labelSpeed1";
            this.labelSpeed1.Size = new System.Drawing.Size(29, 19);
            this.labelSpeed1.TabIndex = 25;
            this.labelSpeed1.Text = "20";
            // 
            // trackBarSetSpeed1
            // 
            this.trackBarSetSpeed1.Location = new System.Drawing.Point(90, 253);
            this.trackBarSetSpeed1.Maximum = 100;
            this.trackBarSetSpeed1.Name = "trackBarSetSpeed1";
            this.trackBarSetSpeed1.Size = new System.Drawing.Size(648, 45);
            this.trackBarSetSpeed1.TabIndex = 24;
            this.trackBarSetSpeed1.Value = 20;
            this.trackBarSetSpeed1.Scroll += new System.EventHandler(this.trackBarSetSpeed1_Scroll);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox3);
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Location = new System.Drawing.Point(104, 4);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Size = new System.Drawing.Size(804, 478);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Robot";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.buttonLoadForTeach);
            this.groupBox3.Controls.Add(this.buttonReadyForPick);
            this.groupBox3.Controls.Add(this.buttonCalOffset);
            this.groupBox3.Controls.Add(this.buttonBin);
            this.groupBox3.Controls.Add(this.buttonUnloadAndLoad);
            this.groupBox3.Controls.Add(this.buttonSaveApproach);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.buttonSavePosition);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.buttonCreateXml);
            this.groupBox3.Controls.Add(this.buttonPlace);
            this.groupBox3.Controls.Add(this.buttonLoad);
            this.groupBox3.Controls.Add(this.buttonPick);
            this.groupBox3.Controls.Add(this.buttonUnload);
            this.groupBox3.Controls.Add(this.comboBoxMovePos);
            this.groupBox3.Controls.Add(this.comboBox_Gripper);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(14, 270);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox3.Size = new System.Drawing.Size(733, 195);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Run";
            // 
            // buttonLoadForTeach
            // 
            this.buttonLoadForTeach.Location = new System.Drawing.Point(406, 58);
            this.buttonLoadForTeach.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonLoadForTeach.Name = "buttonLoadForTeach";
            this.buttonLoadForTeach.Size = new System.Drawing.Size(89, 28);
            this.buttonLoadForTeach.TabIndex = 38;
            this.buttonLoadForTeach.Text = "Teach Load";
            this.buttonLoadForTeach.UseVisualStyleBackColor = true;
            this.buttonLoadForTeach.Click += new System.EventHandler(this.buttonLoadForTeach_Click);
            // 
            // buttonReadyForPick
            // 
            this.buttonReadyForPick.Location = new System.Drawing.Point(406, 19);
            this.buttonReadyForPick.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonReadyForPick.Name = "buttonReadyForPick";
            this.buttonReadyForPick.Size = new System.Drawing.Size(89, 28);
            this.buttonReadyForPick.TabIndex = 37;
            this.buttonReadyForPick.Text = "Ready Phone";
            this.buttonReadyForPick.UseVisualStyleBackColor = true;
            this.buttonReadyForPick.Click += new System.EventHandler(this.buttonReadyForPick_Click);
            // 
            // buttonCalOffset
            // 
            this.buttonCalOffset.Location = new System.Drawing.Point(508, 93);
            this.buttonCalOffset.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonCalOffset.Name = "buttonCalOffset";
            this.buttonCalOffset.Size = new System.Drawing.Size(120, 28);
            this.buttonCalOffset.TabIndex = 36;
            this.buttonCalOffset.Text = "Calculate Offset";
            this.buttonCalOffset.UseVisualStyleBackColor = true;
            this.buttonCalOffset.Click += new System.EventHandler(this.buttonCalOffset_Click);
            // 
            // buttonBin
            // 
            this.buttonBin.Location = new System.Drawing.Point(191, 94);
            this.buttonBin.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonBin.Name = "buttonBin";
            this.buttonBin.Size = new System.Drawing.Size(89, 28);
            this.buttonBin.TabIndex = 35;
            this.buttonBin.Text = "Bin";
            this.buttonBin.UseVisualStyleBackColor = true;
            this.buttonBin.Click += new System.EventHandler(this.buttonBin_Click);
            // 
            // buttonUnloadAndLoad
            // 
            this.buttonUnloadAndLoad.Location = new System.Drawing.Point(286, 94);
            this.buttonUnloadAndLoad.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonUnloadAndLoad.Name = "buttonUnloadAndLoad";
            this.buttonUnloadAndLoad.Size = new System.Drawing.Size(89, 28);
            this.buttonUnloadAndLoad.TabIndex = 34;
            this.buttonUnloadAndLoad.Text = "Unload N Load";
            this.buttonUnloadAndLoad.UseVisualStyleBackColor = true;
            this.buttonUnloadAndLoad.Click += new System.EventHandler(this.buttonUnloadAndLoad_Click);
            // 
            // buttonSaveApproach
            // 
            this.buttonSaveApproach.Location = new System.Drawing.Point(508, 57);
            this.buttonSaveApproach.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonSaveApproach.Name = "buttonSaveApproach";
            this.buttonSaveApproach.Size = new System.Drawing.Size(120, 28);
            this.buttonSaveApproach.TabIndex = 30;
            this.buttonSaveApproach.Text = "Save Approach";
            this.buttonSaveApproach.UseVisualStyleBackColor = true;
            this.buttonSaveApproach.Click += new System.EventHandler(this.buttonSaveApproach_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(154, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 12);
            this.label1.TabIndex = 31;
            // 
            // buttonSavePosition
            // 
            this.buttonSavePosition.Location = new System.Drawing.Point(508, 18);
            this.buttonSavePosition.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonSavePosition.Name = "buttonSavePosition";
            this.buttonSavePosition.Size = new System.Drawing.Size(118, 28);
            this.buttonSavePosition.TabIndex = 29;
            this.buttonSavePosition.Text = "Save Position";
            this.buttonSavePosition.UseVisualStyleBackColor = true;
            this.buttonSavePosition.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 32;
            this.label2.Text = "Gripper:";
            // 
            // buttonCreateXml
            // 
            this.buttonCreateXml.Location = new System.Drawing.Point(662, 18);
            this.buttonCreateXml.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonCreateXml.Name = "buttonCreateXml";
            this.buttonCreateXml.Size = new System.Drawing.Size(55, 46);
            this.buttonCreateXml.TabIndex = 27;
            this.buttonCreateXml.Text = "Xml";
            this.buttonCreateXml.UseVisualStyleBackColor = true;
            this.buttonCreateXml.Click += new System.EventHandler(this.buttonCreateXml_Click);
            // 
            // buttonPlace
            // 
            this.buttonPlace.Location = new System.Drawing.Point(191, 55);
            this.buttonPlace.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonPlace.Name = "buttonPlace";
            this.buttonPlace.Size = new System.Drawing.Size(89, 28);
            this.buttonPlace.TabIndex = 25;
            this.buttonPlace.Text = "Place";
            this.buttonPlace.UseVisualStyleBackColor = true;
            this.buttonPlace.Click += new System.EventHandler(this.buttonPlace_Click);
            // 
            // buttonLoad
            // 
            this.buttonLoad.Location = new System.Drawing.Point(286, 18);
            this.buttonLoad.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(89, 28);
            this.buttonLoad.TabIndex = 21;
            this.buttonLoad.Text = "Load";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // buttonPick
            // 
            this.buttonPick.Location = new System.Drawing.Point(192, 16);
            this.buttonPick.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonPick.Name = "buttonPick";
            this.buttonPick.Size = new System.Drawing.Size(89, 28);
            this.buttonPick.TabIndex = 24;
            this.buttonPick.Text = "Pick";
            this.buttonPick.UseVisualStyleBackColor = true;
            this.buttonPick.Click += new System.EventHandler(this.buttonPick_Click);
            // 
            // buttonUnload
            // 
            this.buttonUnload.Location = new System.Drawing.Point(286, 56);
            this.buttonUnload.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonUnload.Name = "buttonUnload";
            this.buttonUnload.Size = new System.Drawing.Size(89, 28);
            this.buttonUnload.TabIndex = 26;
            this.buttonUnload.Text = "Unload";
            this.buttonUnload.UseVisualStyleBackColor = true;
            this.buttonUnload.Click += new System.EventHandler(this.buttonUnload_Click);
            // 
            // comboBoxMovePos
            // 
            this.comboBoxMovePos.FormattingEnabled = true;
            this.comboBoxMovePos.Location = new System.Drawing.Point(68, 47);
            this.comboBoxMovePos.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBoxMovePos.Name = "comboBoxMovePos";
            this.comboBoxMovePos.Size = new System.Drawing.Size(120, 20);
            this.comboBoxMovePos.TabIndex = 23;
            this.comboBoxMovePos.SelectedIndexChanged += new System.EventHandler(this.comboBoxMovePos_SelectedIndexChanged);
            // 
            // comboBox_Gripper
            // 
            this.comboBox_Gripper.FormattingEnabled = true;
            this.comboBox_Gripper.Location = new System.Drawing.Point(68, 16);
            this.comboBox_Gripper.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBox_Gripper.Name = "comboBox_Gripper";
            this.comboBox_Gripper.Size = new System.Drawing.Size(120, 20);
            this.comboBox_Gripper.TabIndex = 22;
            this.comboBox_Gripper.SelectedIndexChanged += new System.EventHandler(this.comboBox_Gripper_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 33;
            this.label3.Text = "Target:";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.buttonForPicked);
            this.tabPage4.Controls.Add(this.buttonForReady);
            this.tabPage4.Controls.Add(this.buttonForInpos);
            this.tabPage4.Controls.Add(this.checkBoxPickConveyorMoveForward);
            this.tabPage4.Controls.Add(this.buttonConveyorStart);
            this.tabPage4.Location = new System.Drawing.Point(104, 4);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(804, 478);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Conveyor";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // buttonForPicked
            // 
            this.buttonForPicked.Location = new System.Drawing.Point(38, 171);
            this.buttonForPicked.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonForPicked.Name = "buttonForPicked";
            this.buttonForPicked.Size = new System.Drawing.Size(76, 26);
            this.buttonForPicked.TabIndex = 4;
            this.buttonForPicked.Text = "Picked";
            this.buttonForPicked.UseVisualStyleBackColor = true;
            this.buttonForPicked.Click += new System.EventHandler(this.button41_Click);
            // 
            // buttonForReady
            // 
            this.buttonForReady.Location = new System.Drawing.Point(38, 132);
            this.buttonForReady.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonForReady.Name = "buttonForReady";
            this.buttonForReady.Size = new System.Drawing.Size(76, 26);
            this.buttonForReady.TabIndex = 3;
            this.buttonForReady.Text = "Ready";
            this.buttonForReady.UseVisualStyleBackColor = true;
            this.buttonForReady.Click += new System.EventHandler(this.button40_Click);
            // 
            // buttonForInpos
            // 
            this.buttonForInpos.Location = new System.Drawing.Point(38, 93);
            this.buttonForInpos.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonForInpos.Name = "buttonForInpos";
            this.buttonForInpos.Size = new System.Drawing.Size(76, 26);
            this.buttonForInpos.TabIndex = 2;
            this.buttonForInpos.Text = "Inpos";
            this.buttonForInpos.UseVisualStyleBackColor = true;
            this.buttonForInpos.Click += new System.EventHandler(this.button39_Click);
            // 
            // checkBoxPickConveyorMoveForward
            // 
            this.checkBoxPickConveyorMoveForward.AutoSize = true;
            this.checkBoxPickConveyorMoveForward.Checked = true;
            this.checkBoxPickConveyorMoveForward.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPickConveyorMoveForward.Location = new System.Drawing.Point(38, 20);
            this.checkBoxPickConveyorMoveForward.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBoxPickConveyorMoveForward.Name = "checkBoxPickConveyorMoveForward";
            this.checkBoxPickConveyorMoveForward.Size = new System.Drawing.Size(150, 16);
            this.checkBoxPickConveyorMoveForward.TabIndex = 1;
            this.checkBoxPickConveyorMoveForward.Text = "Conveyor move forward";
            this.checkBoxPickConveyorMoveForward.UseVisualStyleBackColor = true;
            this.checkBoxPickConveyorMoveForward.CheckedChanged += new System.EventHandler(this.checkBoxPickConveyorMoveForward_CheckedChanged);
            // 
            // buttonConveyorStart
            // 
            this.buttonConveyorStart.Location = new System.Drawing.Point(38, 53);
            this.buttonConveyorStart.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonConveyorStart.Name = "buttonConveyorStart";
            this.buttonConveyorStart.Size = new System.Drawing.Size(76, 26);
            this.buttonConveyorStart.TabIndex = 0;
            this.buttonConveyorStart.Text = "Start";
            this.buttonConveyorStart.UseVisualStyleBackColor = true;
            this.buttonConveyorStart.Click += new System.EventHandler(this.button38_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.buttonBoxSave);
            this.tabPage3.Controls.Add(this.buttonBoxLoad);
            this.tabPage3.Controls.Add(this.buttonOK6);
            this.tabPage3.Controls.Add(this.comboBox6);
            this.tabPage3.Controls.Add(this.buttonOK5);
            this.tabPage3.Controls.Add(this.comboBox5);
            this.tabPage3.Controls.Add(this.buttonOK4);
            this.tabPage3.Controls.Add(this.comboBox4);
            this.tabPage3.Controls.Add(this.buttonOK3);
            this.tabPage3.Controls.Add(this.comboBox3);
            this.tabPage3.Controls.Add(this.buttonOK2);
            this.tabPage3.Controls.Add(this.comboBox2);
            this.tabPage3.Controls.Add(this.buttonOK1);
            this.tabPage3.Controls.Add(this.comboBox1);
            this.tabPage3.Controls.Add(this.listViewBox);
            this.tabPage3.Location = new System.Drawing.Point(104, 4);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(804, 478);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Box";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            this.tabPage5.Location = new System.Drawing.Point(104, 4);
            this.tabPage5.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(804, 478);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Tester";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // tabPage6
            // 
            this.tabPage6.Location = new System.Drawing.Point(104, 4);
            this.tabPage6.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Size = new System.Drawing.Size(804, 478);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "Log";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // tabPage7
            // 
            this.tabPage7.Controls.Add(this.label8);
            this.tabPage7.Location = new System.Drawing.Point(104, 4);
            this.tabPage7.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Size = new System.Drawing.Size(804, 478);
            this.tabPage7.TabIndex = 6;
            this.tabPage7.Text = "Setting";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(48, 35);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 0;
            this.label8.Text = "账户";
            // 
            // richTextBoxMessage
            // 
            this.richTextBoxMessage.Location = new System.Drawing.Point(99, 10);
            this.richTextBoxMessage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.richTextBoxMessage.Name = "richTextBoxMessage";
            this.richTextBoxMessage.Size = new System.Drawing.Size(835, 55);
            this.richTextBoxMessage.TabIndex = 19;
            this.richTextBoxMessage.Text = "当前设备状态，报警，等待手机，执行任务，解决办法";
            // 
            // listViewBox
            // 
            this.listViewBox.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewBox.ContextMenuStrip = this.contextMenuStrip1;
            listViewGroup1.Header = "Up";
            listViewGroup1.Name = "上层";
            listViewGroup2.Header = "Down";
            listViewGroup2.Name = "下层";
            this.listViewBox.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            listViewItem1.Group = listViewGroup1;
            listViewItem2.Group = listViewGroup1;
            listViewItem3.Group = listViewGroup1;
            listViewItem4.Group = listViewGroup2;
            listViewItem5.Group = listViewGroup2;
            listViewItem6.Group = listViewGroup2;
            this.listViewBox.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5,
            listViewItem6});
            this.listViewBox.LargeImageList = this.imageList1;
            this.listViewBox.Location = new System.Drawing.Point(1, 0);
            this.listViewBox.Name = "listViewBox";
            this.listViewBox.Size = new System.Drawing.Size(804, 475);
            this.listViewBox.TabIndex = 0;
            this.listViewBox.UseCompatibleStateImageBehavior = false;
            this.listViewBox.SelectedIndexChanged += new System.EventHandler(this.listViewBox_SelectedIndexChanged);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "t015257e7479fcfed87.png");
            this.imageList1.Images.SetKeyName(1, "t015257e7479fcfed87.png");
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(22, 154);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(56, 20);
            this.comboBox1.TabIndex = 1;
            // 
            // buttonOK1
            // 
            this.buttonOK1.Location = new System.Drawing.Point(84, 154);
            this.buttonOK1.Name = "buttonOK1";
            this.buttonOK1.Size = new System.Drawing.Size(45, 23);
            this.buttonOK1.TabIndex = 2;
            this.buttonOK1.Text = "OK";
            this.buttonOK1.UseVisualStyleBackColor = true;
            this.buttonOK1.Click += new System.EventHandler(this.buttonOK1_Click);
            // 
            // buttonOK2
            // 
            this.buttonOK2.Location = new System.Drawing.Point(228, 154);
            this.buttonOK2.Name = "buttonOK2";
            this.buttonOK2.Size = new System.Drawing.Size(45, 23);
            this.buttonOK2.TabIndex = 4;
            this.buttonOK2.Text = "OK";
            this.buttonOK2.UseVisualStyleBackColor = true;
            this.buttonOK2.Click += new System.EventHandler(this.buttonOK2_Click);
            // 
            // comboBox2
            // 
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(166, 154);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(56, 20);
            this.comboBox2.TabIndex = 3;
            // 
            // buttonOK3
            // 
            this.buttonOK3.Location = new System.Drawing.Point(371, 154);
            this.buttonOK3.Name = "buttonOK3";
            this.buttonOK3.Size = new System.Drawing.Size(45, 23);
            this.buttonOK3.TabIndex = 6;
            this.buttonOK3.Text = "OK";
            this.buttonOK3.UseVisualStyleBackColor = true;
            this.buttonOK3.Click += new System.EventHandler(this.buttonOK3_Click);
            // 
            // comboBox3
            // 
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Location = new System.Drawing.Point(309, 154);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(56, 20);
            this.comboBox3.TabIndex = 5;
            // 
            // buttonOK4
            // 
            this.buttonOK4.Location = new System.Drawing.Point(84, 324);
            this.buttonOK4.Name = "buttonOK4";
            this.buttonOK4.Size = new System.Drawing.Size(45, 23);
            this.buttonOK4.TabIndex = 8;
            this.buttonOK4.Text = "OK";
            this.buttonOK4.UseVisualStyleBackColor = true;
            this.buttonOK4.Click += new System.EventHandler(this.buttonOK4_Click);
            // 
            // comboBox4
            // 
            this.comboBox4.FormattingEnabled = true;
            this.comboBox4.Location = new System.Drawing.Point(22, 324);
            this.comboBox4.Name = "comboBox4";
            this.comboBox4.Size = new System.Drawing.Size(56, 20);
            this.comboBox4.TabIndex = 7;
            // 
            // buttonOK5
            // 
            this.buttonOK5.Location = new System.Drawing.Point(228, 327);
            this.buttonOK5.Name = "buttonOK5";
            this.buttonOK5.Size = new System.Drawing.Size(45, 23);
            this.buttonOK5.TabIndex = 10;
            this.buttonOK5.Text = "OK";
            this.buttonOK5.UseVisualStyleBackColor = true;
            this.buttonOK5.Click += new System.EventHandler(this.buttonOK5_Click);
            // 
            // comboBox5
            // 
            this.comboBox5.FormattingEnabled = true;
            this.comboBox5.Location = new System.Drawing.Point(166, 327);
            this.comboBox5.Name = "comboBox5";
            this.comboBox5.Size = new System.Drawing.Size(56, 20);
            this.comboBox5.TabIndex = 9;
            // 
            // buttonOK6
            // 
            this.buttonOK6.Location = new System.Drawing.Point(371, 326);
            this.buttonOK6.Name = "buttonOK6";
            this.buttonOK6.Size = new System.Drawing.Size(45, 23);
            this.buttonOK6.TabIndex = 12;
            this.buttonOK6.Text = "OK";
            this.buttonOK6.UseVisualStyleBackColor = true;
            this.buttonOK6.Click += new System.EventHandler(this.buttonOK6_Click);
            // 
            // comboBox6
            // 
            this.comboBox6.FormattingEnabled = true;
            this.comboBox6.Location = new System.Drawing.Point(309, 326);
            this.comboBox6.Name = "comboBox6";
            this.comboBox6.Size = new System.Drawing.Size(56, 20);
            this.comboBox6.TabIndex = 11;
            // 
            // buttonBoxLoad
            // 
            this.buttonBoxLoad.Location = new System.Drawing.Point(514, 208);
            this.buttonBoxLoad.Name = "buttonBoxLoad";
            this.buttonBoxLoad.Size = new System.Drawing.Size(76, 40);
            this.buttonBoxLoad.TabIndex = 13;
            this.buttonBoxLoad.Text = "Load";
            this.buttonBoxLoad.UseVisualStyleBackColor = true;
            this.buttonBoxLoad.Click += new System.EventHandler(this.buttonBoxLoad_Click);
            // 
            // buttonBoxSave
            // 
            this.buttonBoxSave.Location = new System.Drawing.Point(514, 279);
            this.buttonBoxSave.Name = "buttonBoxSave";
            this.buttonBoxSave.Size = new System.Drawing.Size(76, 40);
            this.buttonBoxSave.TabIndex = 14;
            this.buttonBoxSave.Text = "Save";
            this.buttonBoxSave.UseVisualStyleBackColor = true;
            this.buttonBoxSave.Click += new System.EventHandler(this.buttonBoxSave_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.启用ToolStripMenuItem,
            this.禁用ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(101, 48);
            // 
            // 启用ToolStripMenuItem
            // 
            this.启用ToolStripMenuItem.Name = "启用ToolStripMenuItem";
            this.启用ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.启用ToolStripMenuItem.Text = "启用";
            this.启用ToolStripMenuItem.Click += new System.EventHandler(this.启用ToolStripMenuItem_Click);
            // 
            // 禁用ToolStripMenuItem
            // 
            this.禁用ToolStripMenuItem.Name = "禁用ToolStripMenuItem";
            this.禁用ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.禁用ToolStripMenuItem.Text = "禁用";
            this.禁用ToolStripMenuItem.Click += new System.EventHandler(this.禁用ToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(942, 564);
            this.Controls.Add(this.richTextBoxMessage);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "Rack";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSetSpeed2)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSetSpeed1)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage7.ResumeLayout(false);
            this.tabPage7.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonHome;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.CheckBox checkBoxIsLoop;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelPositionG1;
        private System.Windows.Forms.Label labelPositionR;
        private System.Windows.Forms.Label labelPositionZ;
        private System.Windows.Forms.Label labelPositionY;
        private System.Windows.Forms.Label labelPositionX;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonPositiveZ;
        private System.Windows.Forms.Button buttonNagetiveZ;
        private System.Windows.Forms.Button buttonPositiveY;
        private System.Windows.Forms.Button buttonNagetiveY;
        private System.Windows.Forms.Button buttonPositiveX2;
        private System.Windows.Forms.Button buttonNagetiveX2;
        private System.Windows.Forms.Button buttonPositiveX1;
        private System.Windows.Forms.Button buttonNagetiveX1;
        private System.Windows.Forms.Button buttonPositiveR;
        private System.Windows.Forms.Button buttonNagetiveR;
        private System.Windows.Forms.Button buttonRunG2;
        private System.Windows.Forms.Button buttonRunG1;
        private System.Windows.Forms.TextBox textBoxDistanceG2;
        private System.Windows.Forms.TextBox textBoxDistanceG1;
        private System.Windows.Forms.Button buttonEableG2;
        private System.Windows.Forms.Button buttonEableG1;
        private System.Windows.Forms.Label labelPositionG2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.TabPage tabPage7;
        private System.Windows.Forms.RichTextBox richTextBoxMessage;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button buttonEableR;
        private System.Windows.Forms.Button buttonEableZ;
        private System.Windows.Forms.Button buttonEableY;
        private System.Windows.Forms.Button buttonEableX2;
        private System.Windows.Forms.Button buttonEableX1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button buttonUnload;
        private System.Windows.Forms.Button buttonPlace;
        private System.Windows.Forms.Button buttonPick;
        private System.Windows.Forms.ComboBox comboBoxMovePos;
        private System.Windows.Forms.ComboBox comboBox_Gripper;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.Button buttonCreateXml;
        private System.Windows.Forms.Button buttonSaveApproach;
        private System.Windows.Forms.Button buttonSavePosition;
        private System.Windows.Forms.Button buttonConveyorStart;
        private System.Windows.Forms.CheckBox checkBoxPickConveyorMoveForward;
        private System.Windows.Forms.Button buttonForInpos;
        private System.Windows.Forms.Button buttonForReady;
        private System.Windows.Forms.Button buttonForPicked;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label labelSpeed2;
        private System.Windows.Forms.TrackBar trackBarSetSpeed2;
        private System.Windows.Forms.Label labelSpeed1;
        private System.Windows.Forms.TrackBar trackBarSetSpeed1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonLowSpeed;
        private System.Windows.Forms.Button buttonMiddleSpeed;
        private System.Windows.Forms.Button buttonHighSpeed;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonG1TightOrLoose;
        private System.Windows.Forms.Button buttonG2TightOrLoose;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonBin;
        private System.Windows.Forms.Button buttonUnloadAndLoad;
        private System.Windows.Forms.Button buttonCalOffset;
        private System.Windows.Forms.Button buttonReadyForPick;
        private System.Windows.Forms.Button buttonLoadForTeach;
        private System.Windows.Forms.Button buttonBoxSave;
        private System.Windows.Forms.Button buttonBoxLoad;
        private System.Windows.Forms.Button buttonOK6;
        private System.Windows.Forms.ComboBox comboBox6;
        private System.Windows.Forms.Button buttonOK5;
        private System.Windows.Forms.ComboBox comboBox5;
        private System.Windows.Forms.Button buttonOK4;
        private System.Windows.Forms.ComboBox comboBox4;
        private System.Windows.Forms.Button buttonOK3;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.Button buttonOK2;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Button buttonOK1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ListView listViewBox;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 启用ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 禁用ToolStripMenuItem;
    }
}

