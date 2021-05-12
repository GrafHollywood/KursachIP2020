using System;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;

namespace KursachIP
{
    public partial class MainForm : Form
    {
        //длина балки
        private double L { set; get; }
        //размеры участков балки
        private double A { set; get; }
        private double B { set; get; }
        private double C { set; get; }
        private double D { set; get; }
        private double E { set; get; }
        private double M { set; get; }
        private double Q1 { set; get; }
        private double Q2 { set; get; }
        private double Qmax { set; get; }
        //модуль Юнга
        private const long ModulElastic = 71000L * 1000000L;
        //плотность
        private const double MaterialDensity = 2.67 * 1000;
        
        //размеры сечений балки
        private double SectionH { set; get; }
        private double SectionB { set; get; }
        private double SectionH1 { set; get; }
        private double SectionB1 { set; get; }

        //инициализация формы. присваивание названий графикам
        public MainForm()
        {
            InitializeComponent();

            GraphPane pane1 = zedGraphControlForce.GraphPane;
            pane1.Title.Text = "Эпюра поперечных сил";
            pane1.XAxis.Title.Text = "mm";
            pane1.YAxis.Title.Text = "N";
            pane1.YAxis.MajorGrid.IsVisible = true;

            GraphPane pane2 = zedGraphControlMoment.GraphPane;
            pane2.Title.Text = "Эпюра изгибающих моментов";
            pane2.XAxis.Title.Text = "mm";
            pane2.YAxis.Title.Text = "N x m";
            pane2.YAxis.MajorGrid.IsVisible = true;

            GraphPane pane3 = zedGraphControlDeflection1.GraphPane;
            pane3.Title.Text = "Функция прогиба срединной линии балки сплошного сечения";
            pane3.XAxis.Title.Text = "mm";
            pane3.YAxis.Title.Text = "mm";
            pane3.XAxis.MajorGrid.IsVisible = true;
            pane3.YAxis.MajorGrid.IsVisible = true;

            GraphPane pane4 = zedGraphControlAngle1.GraphPane;
            pane4.Title.Text = "Функция угла поворота по срединной линии балки сплошного сечения";
            pane4.XAxis.Title.Text = "mm";
            pane4.YAxis.Title.Text = "рад";
            pane4.XAxis.MajorGrid.IsVisible = true;
            pane4.YAxis.MajorGrid.IsVisible = true;

            GraphPane pane7 = zedGraphControlMaxStress.GraphPane;
            pane7.Title.Text = "Изменение максимального напряжения от нагрузки";
            pane7.XAxis.Title.Text = "Н";
            pane7.YAxis.Title.Text = "МПа";
            pane7.XAxis.MajorGrid.IsVisible = true;
            pane7.YAxis.MajorGrid.IsVisible = true;

            GraphPane pane8 = zedGraphControlMaxDeflection.GraphPane;
            pane8.Title.Text = "Изменение максимального прогиба от нагрузки";
            pane8.XAxis.Title.Text = "mm";
            pane8.YAxis.Title.Text = "mm";
            pane8.XAxis.MajorGrid.IsVisible = true;
            pane8.YAxis.MajorGrid.IsVisible = true;

            GraphPane pane9 = zedGraphControlWeight.GraphPane;
            pane9.Title.Text = "Изменение максимального напряжения от массы полого сечения";
            pane9.XAxis.Title.Text = "кг";
            pane9.YAxis.Title.Text = "МПа";
            pane9.XAxis.MajorGrid.IsVisible = true;
            pane9.YAxis.MajorGrid.IsVisible = true;
        }

        //получение данных из полей. проверка на корректность
        //если есть ошибка возвращает false
        private bool CheckData()
        {
            A = int.Parse(textBoxA.Text) * 0.001;
            B = int.Parse(textBoxB.Text) * 0.001;
            C = int.Parse(textBoxC.Text) * 0.001;
            D = int.Parse(textBoxD.Text) * 0.001;
            E = int.Parse(textBoxE.Text) * 0.001;
            L = A + B + C + D + E;

            M = int.Parse(textBoxM.Text);
            Q1 = int.Parse(textBoxQ1.Text);
            Q2 = int.Parse(textBoxQ2.Text);
            Qmax = int.Parse(textBoxQmax.Text);

            SectionH = int.Parse(textBoxSectionH.Text) * 0.001;
            SectionB = int.Parse(textBoxSectionB.Text) * 0.001;
            SectionH1 = int.Parse(textBoxSectionH1.Text) * 0.001;
            SectionB1 = int.Parse(textBoxSectionB1.Text) * 0.001;

            if (Q1 < Q2)
            {
                MessageBox.Show("Q1 не может быть меньше или равна Q2!!!");
                return false;
            }
            if (L == 0)
            {
                MessageBox.Show("Балка не может иметь нулевую длину!!!");
                return false;
            }
            if (SectionH <= SectionH1)
            {
                MessageBox.Show("H не может быть меньше или равно H1!!!");
                return false;
            }
            if (SectionB <= SectionB1)
            {
                MessageBox.Show("B не может быть меньше или равно B1!!!");
                return false;
            }
            return true;
        }

        //обработчик событий для полей, который не дает пользователю вводить иные символы кроме цифр
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsNumber(number) && !Char.IsControl(number))
            {
                e.Handled = true;
            }
        }

        //построение эпюр поперечной силы и изгибающего момента
        private void button1_Click(object sender, EventArgs e) 
        {
            //получение данных
            if (!CheckData())
                return;

            GraphPane pane1 = zedGraphControlForce.GraphPane;
            GraphPane pane2 = zedGraphControlMoment.GraphPane;
            //очистка окна вывода графика
            pane1.CurveList.Clear();
            pane2.CurveList.Clear();

            //построение эпюры поперечных сил
            double maxQ = FunctionTransverseForces(0);
            double minQ = FunctionTransverseForces(0);
            PointPairList list1 = new PointPairList();
            for (double i = 0; i <= L; i += 0.001)
            {
                double q = FunctionTransverseForces(i);
                if (q >= maxQ)
                    maxQ = q;
                else if (q <= minQ)
                    minQ = q;
                list1.Add(i * 1000, q);
            }
            //построение эпюры изгибающего момента
            double maxM = FunctionBendingMoment(0, Qmax, Q1, Q2, M);
            double minM = FunctionBendingMoment(0, Qmax, Q1, Q2, M);
            PointPairList list2 = new PointPairList();
            for (double i = 0; i <= L; i += 0.001)
            {
                double m = FunctionBendingMoment(i, Qmax, Q1, Q2, M);
                if (m >= maxM)
                    maxM = m;
                else if (m <= minM)
                    minM = m;
                list2.Add(i * 1000, m);
            }
            
            //вывод графиков
            LineItem myCurve1 = pane1.AddCurve("", list1, Color.Black, SymbolType.None);
            LineItem myCurve2 = pane2.AddCurve("", list2, Color.Black, SymbolType.None);
            //заливка под графиком
            myCurve1.Line.Fill = new Fill(Color.Red);
            myCurve2.Line.Fill = new Fill(Color.Red);
            //толщина линии
            myCurve1.Line.Width = 3;
            myCurve2.Line.Width = 3;

            //изменения предела вывода графика
            zedGraphControlForce.AxisChange();
            pane1.XAxis.Scale.Min = 0;
            pane1.XAxis.Scale.Max = L * 1000;
            pane1.YAxis.Scale.Min = minQ;
            pane1.YAxis.Scale.Max = maxQ;
            zedGraphControlForce.Invalidate();//обновление графика

            zedGraphControlMoment.AxisChange();
            pane2.XAxis.Scale.Min = 0;
            pane2.XAxis.Scale.Max = L * 1000;
            pane2.YAxis.Scale.Min = minM;
            pane2.YAxis.Scale.Max = maxM;
            zedGraphControlMoment.Invalidate();

            //нарисуем линии показывающие участки на эпюрах поперечной силы
            LineObj lineA = new LineObj(A * 1000, minQ, A * 1000, maxQ);
            lineA.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
            pane1.GraphObjList.Add(lineA);
            LineObj lineB = new LineObj((A + B) * 1000, minQ, (A + B) * 1000, maxQ);
            lineB.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
            pane1.GraphObjList.Add(lineB);
            LineObj lineC = new LineObj((A + B + C) * 1000, minQ, (A + B + C) * 1000, maxQ);
            lineC.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
            pane1.GraphObjList.Add(lineC);
            LineObj lineD = new LineObj((A + B + C + D) * 1000, minQ, (A + B + C + D) * 1000, maxQ);
            lineD.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
            pane1.GraphObjList.Add(lineD);

            //нарисуем линии показывающие участки на эпюрах изгибающего момента
            LineObj lineMA = new LineObj(A * 1000, minM, A * 1000, maxM);
            lineMA.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
            pane2.GraphObjList.Add(lineMA);
            LineObj lineMB = new LineObj((A + B) * 1000, minM, (A + B) * 1000, maxM);
            lineMB.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
            pane2.GraphObjList.Add(lineMB);
            LineObj lineMC = new LineObj((A + B+C) * 1000, minM, (A + B+C) * 1000, maxM);
            lineMC.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
            pane2.GraphObjList.Add(lineMC);
            LineObj lineMD = new LineObj((A + B+C+D) * 1000, minM, (A + B+C+D) * 1000, maxM);
            lineMD.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
            pane2.GraphObjList.Add(lineMD);

        }

        //построение прогиба и углов поворота 
        private void button2_Click(object sender, EventArgs e)
        {
            //получение данных
            if (!CheckData())
                return;

            //получение моментов инерции сечений
            double J1 = GetGeometricCharacteristics(SectionB, SectionH, 0, 0);//сплошное сечение
            double J2 = GetGeometricCharacteristics(SectionB, SectionH, SectionB1, SectionH1);//полое сечение

            GraphPane pane1 = zedGraphControlDeflection1.GraphPane;
            pane1.CurveList.Clear();
            GraphPane pane2 = zedGraphControlAngle1.GraphPane;
            pane2.CurveList.Clear();

            //построение прогиба для сплошного сечения
            PointPairList list11 = new PointPairList();
            for (double i = 0; i <= L; i += 0.001)
            {
                double x = i * 1000;
                double y = FunctionDeflection(i, Qmax, Q1, Q2, M, J1);
                list11.Add(x, y * 1000);
            }

            //построение прогиба для полого сечения
            PointPairList list12 = new PointPairList();
            for (double i = 0; i <= L; i += 0.001)
            {
                double x = i * 1000;
                double y = FunctionDeflection(i, Qmax, Q1, Q2, M, J2);
                list12.Add(x, y * 1000);
            }

            //вывод графиков на экран
            LineItem myCurve1 = pane1.AddCurve("Сплошное сечение", list11, Color.Blue, SymbolType.None);
            myCurve1.Line.Width = 3;
            LineItem myCurve3 = pane1.AddCurve("Полое сечение", list12, Color.Red, SymbolType.None);
            myCurve3.Line.Width = 3;

            //построение угла поворота для сплошного сечения
            PointPairList list21 = new PointPairList();
            for (double i = 0; i <= L; i += 0.001)
            {
                double x = i * 1000;
                double y = FunctionAngleDeflection(i, J1);
                list21.Add(x, y);
            }

            //построение угла поворота для полого сечения
            PointPairList list22 = new PointPairList();
            for (double i = 0; i <= L; i += 0.001)
            {
                double x = i * 1000;
                double y = FunctionAngleDeflection(i, J2);
                list22.Add(x, y);
            }

            //вывод графиков на экран
            LineItem myCurve2 = pane2.AddCurve("Сплошное сечение", list21, Color.Blue, SymbolType.None);
            myCurve2.Line.Fill = new Fill(Color.Blue);
            LineItem myCurve4 = pane2.AddCurve("Полое сечение", list22, Color.Red, SymbolType.None);
            myCurve4.Line.Fill = new Fill(Color.Red);

            zedGraphControlDeflection1.AxisChange();
            pane1.XAxis.Scale.Min = 0;
            pane1.XAxis.Scale.Max = L * 1000;
            zedGraphControlDeflection1.Invalidate();

            zedGraphControlAngle1.AxisChange();
            pane2.XAxis.Scale.Min = 0;
            pane2.XAxis.Scale.Max = L * 1000;
            zedGraphControlAngle1.Invalidate();
        }

        //функция поперечных сил балки
        //возвращает значение поперечной силы в точке z
        //в параметрах указывается точка в которой необходимо определить поперечную силу
        private double FunctionTransverseForces(double z)
        {
            if (z <= A + B)
            {
                //участки A и B
                return -(Qmax * z * z) / (2 * (A + B));
            }
            if (z >= A + B && z <= A + B + C)
            {
                //участок С
                return -(Qmax * (A + B)) / 2;
            }
            if (z >= A + B + C && z <= A + B + C + D)
            {
                //участок D
                double q = -(Qmax * (A + B)) / 2;
                double dif = Q1 - Q2;
                return q - Q2 * (z - A - B - C) - (dif * D) / 2 + (Math.Pow(z - A - B - C - D, 2) * dif) / (2 * D);
            }
            if (z >= A + B + C + D && z <= L)
            {
                //участок E
                double q = -(Qmax * (A + B)) / 2;
                return q - (Q1 + Q2) / 2 * D;
            }
            return 0;
        }

        //функция изгибающих моментов балки
        //возвращает значение изгибающего момента в точке z
        //в параметрах указывается точка в которой необходимо определить изгибающий момент и нагрузки балки (необходимо при построении графиков изменения максимального прогиба и напряжения)
        private double FunctionBendingMoment(double z, double Qmax, double Q1, double Q2, double M)
        {
            if (z < A)
            {
                //участок A
                return -(Qmax * z * z) / (2 * (A + B)) * (z / 3);
            }
            if (z >= A && z <= A + B)
            {
                //участок B
                return -(Qmax * z * z) / (2 * (A + B)) * (z / 3) - M;
            }
            if (z >= A + B && z <= A + B + C)
            {
                //участок C
                return -(Qmax * (A + B)) / 2 * (z - (2 * (A + B) / 3)) - M;
            }
            if (z >= A + B + C && z <= A + B + C + D)
            {
                //участок D
                double qy1 = -(Qmax * (A + B)) / 2;//поперечная сила от первой распраделенной нагрузки
                double dif = Q1 - Q2;
                double qy2 = -Q2 * (z - A - B - C) - (dif * D) / 2 + (Math.Pow(z - A - B - C - D, 2) * dif) / (2 * D);//поперечная сила от второй распраделенной нагрузки
                return qy1 * (z - (2 * (A + B) / 3)) - M + qy2 * (z - A - B - C) * 2 / 3;
            }
            if (z >= A + B + C + D && z <= L)
            {
                //участок E
                double qy1 = -(Qmax * (A + B)) / 2;//поперечная сила от первой распраделенной нагрузки
                double qy2 = -(Q1 + Q2) / 2 * D;//поперечная сила от второй распраделенной нагрузки
                return qy1 * (z - (2 * (A + B) / 3)) - M + qy2 * (z - A - B - C - D / 3);
            }
            return 0;
        }

        //функция угла поворота балки
        //возвращает значение угла поворота в точке z
        //в параметрах указывается точка в которой необходимо определить угол поворота и момент инерции сечения
        //значение суммируется от всех нагрузок
        private double FunctionAngleDeflection(double z, double J)
        {
            ///угол поворота от момента
            //начальный угол поворота
            //находится из условия, что в заделке угол поворота равен 0
            double t0 = -1 / (ModulElastic * J) * (-M * (L - A));
            double y = 0;
            if (z <= A)
                //слева от момента
                y += t0;
            else if (z >= A && z <= L)
                //справа от момента
                y += t0 + 1 / (ModulElastic * J) * (-M * (z - A));

            ///угол поворота от первой распределенная нагрузки
            double k = Qmax / (A + B);//тангенс угла распределенной нагрузки
            //начальный угол поворота
            //находится из условия, что в заделке угол поворота равен 0
            t0 = -1 / (ModulElastic * J) * (-k * Math.Pow(L, 4) / 24 + k * Math.Pow(L - A - B, 4) / 24 + Qmax * Math.Pow(L - A - B, 3) / 6);
            if (z <= A + B)
                //на участке действия нагрузки
                y += t0 + 1 / (ModulElastic * J) * (-k * Math.Pow(z, 4) / 24);
            else
                //слева от участка действия нагрузки
                y += t0 + 1 / (ModulElastic * J) * (-k * Math.Pow(z, 4) / 24 + k * Math.Pow(z - A - B, 4) / 24 + Qmax * Math.Pow(z - A - B, 3) / 6);

            ///угол поворота от второй распределенная нагрузки
            k = (Q1 - Q2) / D;//тангенс угла распределенной нагрузки
            double l1 = A + B + C,//длина участка до начала нагрузки
                l2 = A + B + C + D;//длина участка до окончания действия нагрузки
            //начальный угол поворота
            //находится из условия, что в заделке угол поворота равен 0
            t0 = -1 / (ModulElastic * J) * (Math.Pow(l1 - L, 3) / 2 * (Q1 / 3 + k * (l1 - L) / 12) - Math.Pow(l2 - L, 3) / 2 * (Q2 / 3 + k * (l2 - L) / 12));

            if (z <= A + B + C)
                //справа от участка действия нагрузки
                y += t0;
            else if (z >= A + B + C && z <= A + B + C + D)
                //на участке действия нагрузки
                y += t0 + 1 / (ModulElastic * J) * (Math.Pow(l1 - z, 3) / 2 * (Q1 / 3 + k * (l1 - z) / 12));
            else
                //слева от участка действия нагрузки
                y += t0 + 1 / (ModulElastic * J) * (Math.Pow(l1 - z, 3) / 2 * (Q1 / 3 + k * (l1 - z) / 12) - Math.Pow(l2 - z, 3) / 2 * (Q2 / 3 + k * (l2 - z) / 12));

            return y;
        }

        //функция прогиба балки
        //возвращает значение прогиба в точке z
        //в параметрах указывается точка в которой необходимо определить прогиб, момент инерции сечения и нагрузки балки (необходимо для построения графиков изменения максимального прогиба)
        private double FunctionDeflection(double z, double Qmax, double Q1, double Q2, double M, double J)
        {
            ///прогиб балки от момента
            //начальный угол поворота
            //находится из условия, что в заделке угол поворота равен 0
            double t0 = -1 / (ModulElastic * J) * (-M * (L - A));
            //начальный прогиб
            //находится из условия, что в заделке прогиб равен 0
            double w0 = -t0 * L - 1 / (ModulElastic * J) * (-M * Math.Pow(L - A, 2) / 2);
            double y = 0;
            if (z <= A)
                //слева от момента
                y += w0 + t0 * z;
            else if (z >= A && z <= L)
                //справа от момента
                y += w0 + t0 * z + 1 / (ModulElastic * J) * (-M * Math.Pow(z - A, 2) / 2);

            ///прогиб балки от первой распределенной нагрузки
            double k = Qmax / (A + B);//тангенс угла распределенной нагрузки
            //начальный угол поворота
            //находится из условия, что в заделке угол поворота равен 0
            t0 = -1 / (ModulElastic * J) * (-k * Math.Pow(L, 4) / 24 + k * Math.Pow(L - A - B, 4) / 24 + Qmax * Math.Pow(L - A - B, 3) / 6);
            //начальный прогиб
            //находится из условия, что в заделке прогиб равен 0
            w0 = -t0 * L - 1 / (ModulElastic * J) * (-k * Math.Pow(L, 5) / 120 + k * Math.Pow(L - A - B, 5) / 120 + Qmax * Math.Pow(L - A - B, 4) / 24);
            if (z <= A + B)
                //на участке действия нагрузки
                y += w0 + t0 * z + 1 / (ModulElastic * J) * (-k * Math.Pow(z, 5) / 120);
            else
                //справа от участка действия нагрузки
                y += w0 + t0 * z + 1 / (ModulElastic * J) * (-k * Math.Pow(z, 5) / 120 + k * Math.Pow(z - A - B, 5) / 120 + Qmax * Math.Pow(z - A - B, 4) / 24);

            ///прогиб балки от второй распределенной нагрузки
            k = (Q1 - Q2) / D;//тангенс угла распределенной нагрузки
            double l1 = A + B + C,
                l2 = A + B + C + D;
            //начальный угол поворота
            //находится из условия, что в заделке угол поворота равен 0
            t0 = -1 / (ModulElastic * J) * (Math.Pow(l1 - L, 3) / 6 * (Q1 + k * (l1 - L) / 4) - Math.Pow(l2 - L, 3) / 6 * (Q2 + k * (l2 - L) / 4));
            //начальный прогиб
            //находится из условия, что в заделке прогиб равен 0
            w0 = -t0 * L - 1 / (ModulElastic * J) * (Math.Pow(l1 - L, 4) / 24 * (Q1 + k * (l1 - L) / 5) - Math.Pow(l2 - L, 4) / 24 * (Q2 + k * (l2 - L) / 5));
            if (z <= A + B + C)
                //справа от участка действия нагрузки
                y += w0 + t0 * z;
            else if (z >= A + B + C && z <= A + B + C + D)
                //на участке действия нагрузки
                y += w0 + t0 * z + 1 / (ModulElastic * J) * (Math.Pow(l1 - z, 4) / 24 * (Q1 + k * (l1 - z) / 5));
            else
                //слева от участка действия нагрузки
                y += w0 + t0 * z + 1 / (ModulElastic * J) * (Math.Pow(l1 - z, 4) / 24 * (Q1 + k * (l1 - z) / 5) - Math.Pow(l2 - z, 4) / 24 * (Q2 + k * (l2 - z) / 5));

            return y;
        }

        //расчет момента инерции для сечения
        //в параметрах указываются размеры сечения (необходимо при построении графика изменения массы)
        //если необходимо посчитать момент инерции сплошной балки, то размеры отверстия указываются 0
        private double GetGeometricCharacteristics(double B, double H, double B1, double H1)
        {
            double J1 = Math.Pow(H, 3) * (50 * B + 39) / 864;
            double J2 = Math.Pow(H1, 3) * (50 * B1 + 39) / 864;
            return J1 - J2;
        }


        //построение графика изменения максимального прогиба
        private void button4_Click(object sender, EventArgs e)
        {
            //получение данных из полей
            if (!CheckData())
                return;

            //получение пределов нагрузки из полей
            double qmax, qmin;
            double.TryParse(textBoxMinQ1.Text, out qmin);
            double.TryParse(textBoxMaxQ1.Text, out qmax);

            if (qmax <= qmin)
                return;

            //получение моментов инерций сечений
            double J1 = GetGeometricCharacteristics(SectionB, SectionH, 0, 0);//сплошное
            double J2 = GetGeometricCharacteristics(SectionB, SectionH, SectionB1, SectionH1);//полое

            GraphPane pane = zedGraphControlMaxDeflection.GraphPane;
            pane.CurveList.Clear();

            //постройка графика изменения максимального прогиба для сплошного сечения
            PointPairList list1 = new PointPairList();
            double y;
            for (double i = qmin; i <= qmax; i++)
            {
                //поскольку максимальный прогиб всегда на свободном конце балки, то находим прогиб на этом конце при изменяемой силе
                y = FunctionDeflection(0, i, Q1, Q2, M, J1);
                list1.Add(i, y * 1000);
            }

            //постройка графика изменения максимального прогиба для полого сечения
            PointPairList list2 = new PointPairList();
            for (double i = qmin; i <= qmax; i++)
            {
                //поскольку максимальный прогиб всегда на свободном конце балки, то находим прогиб на этом конце при изменяемой силе
                y = FunctionDeflection(0, i, Q1, Q2, M, J2);
                list2.Add(i, y * 1000);
            }

            //вывод графиков на экран
            LineItem myCurve1 = pane.AddCurve("Сплошное сечение", list1, Color.Blue, SymbolType.None);
            LineItem myCurve2 = pane.AddCurve("Полое сечение", list2, Color.Red, SymbolType.None);
            myCurve1.Line.Width = 3;
            myCurve2.Line.Width = 3;
            zedGraphControlMaxDeflection.AxisChange();
            pane.XAxis.Scale.Max = qmax;
            pane.XAxis.Scale.Min = qmin;
            zedGraphControlMaxDeflection.Invalidate();
        }

        //построение графика изменения максимального напряжения
        private void button5_Click(object sender, EventArgs e)
        {
            //получение данных из полей
            if (!CheckData())
                return;

            //получение пределов нагрузки из полей
            double qmax, qmin;
            double.TryParse(textBoxMinQ2.Text, out qmin);
            double.TryParse(textBoxMaxQ2.Text, out qmax);
            if (qmax <= qmin)
                return;

            //получение моментов инерций сечений
            double J1 = GetGeometricCharacteristics(SectionB, SectionH, 0, 0);//сплошное
            double J2 = GetGeometricCharacteristics(SectionB, SectionH, SectionB1, SectionH1);//полое

            GraphPane pane = zedGraphControlMaxStress.GraphPane;
            pane.CurveList.Clear();

            //постройка графика изменения максимального напряжения для сплошного сечения
            PointPairList list1 = new PointPairList();
            double y;
            for (double i = qmin; i <= qmax; i++)
            {
                //поскольку максимальный момент всегда в точке заделки балки, то находим напряжение в этой точке при изменяемой силе
                y = FunctionBendingMoment(L, i, Q1, Q2, M) * SectionH / (2 * J1);
                list1.Add(i, y / 1000000);
            }

            //постройка графика изменения максимального напряжения для полого сечения
            PointPairList list2 = new PointPairList();
            for (double i = qmin; i <= qmax; i++)
            {
                //поскольку максимальный момент всегда в точке заделки балки, то находим напряжение в этой точке при изменяемой силе
                y = FunctionBendingMoment(L, i, Q1, Q2, M) * SectionH / (2 * J2);
                list2.Add(i, y / 1000000);
            }

            //вывод графиков на экран
            LineItem myCurve1 = pane.AddCurve("Сплошное сечение", list1, Color.Red, SymbolType.None);
            LineItem myCurve2 = pane.AddCurve("Полое сечение", list2, Color.Blue, SymbolType.None);
            myCurve1.Line.Width = 3;
            myCurve2.Line.Width = 3;
            zedGraphControlMaxStress.AxisChange();
            pane.XAxis.Scale.Max = qmax;
            pane.XAxis.Scale.Min = qmin;

            zedGraphControlMaxStress.Invalidate();
        }

        //расчет массы балки
        //оптимизация балки
        //построение графика изменениия максимального напряжения от массы балки
        private void button3_Click(object sender, EventArgs e)
        {
            //получение данных из полей
            if (!CheckData())
                return;

            //вывод массы балок
            double weight1 = Math.Round(GetWeight(SectionB, SectionH, 0, 0) * 100) / 100;
            double weight2 = Math.Round(GetWeight(SectionB, SectionH, SectionB1, SectionH1) * 100) / 100;
            textBoxWeight1.Text = weight1.ToString();
            textBoxWeight2.Text = weight2.ToString();

            //оптимизация балки
            Optimization();

            //построение графика изменения макс напряжения от массы балки
            GraphPane pane = zedGraphControlWeight.GraphPane;
            pane.CurveList.Clear();
            PointPairList list = new PointPairList();

            //изменение внутренних отверстий балки
            //отверстия постеменно увеличиваются
            //получается что балка изменяется от сплошного сечения к полому сечению (размеры отверстия которые указал пользователь)
            for (double i = 0; i <= 1; i += 0.001)
            {
                double J = GetGeometricCharacteristics(SectionB, SectionH, SectionB1 * i, SectionH1 * i);//получение новый момент инерции сечения
                double weight = GetWeight(SectionB, SectionH, SectionB1 * i, SectionH1 * i);//получение веса балки с новым сечением
                double stress = FunctionBendingMoment(L, Qmax, Q1, Q2, M) * SectionH / (2 * J);//получение максимального напряжения при новом сечении
                list.Add(weight, stress / 1000000);
            }

            //вывод графика на экран
            LineItem myCurve = pane.AddCurve("", list, Color.Black, SymbolType.None);
            myCurve.Line.Width = 2;
            zedGraphControlWeight.AxisChange();
            pane.XAxis.Scale.Max = weight1;
            pane.XAxis.Scale.Min = weight2;
            zedGraphControlWeight.Invalidate();
        }

        //возвращает массу балки при указанных размерах сечения
        //в параметрах указываются размеры сечений (габаритные и размеры эллипсов)
        private double GetWeight(double B, double H, double B1, double H1)
        {
            double A = 3 * B * H / 4 - 3 * B1 * H1 / 4;
            return A * L * MaterialDensity;
        }

        //оптимизация полого размера балки
        private void Optimization()
        {
            //т.к. размеры отверстий не изменяются выводим их на экран без изменений
            textBoxOptimB1.Text = (SectionB1 * 1000).ToString();
            textBoxOptimH1.Text = (SectionH1 * 1000).ToString();

            double b1 = SectionB1;
            double h1 = SectionH1;
            double H, B;//габаритные размеры нового оптимизироваанного сечения
            double k = SectionH / GetGeometricCharacteristics(SectionB, SectionH, 0, 0);
            double weight;//вес балки
            double weightMin = GetWeight(SectionB, SectionH, 0, 0);//минимальный вес балки (за начальное значение берется вес сплошной балки)

            //подбор оптимизированного сечения балки проводится за счет изменения габаритной высоты сечения
            //габаритная ширина зависит от высоты из условия равности напряжений
            for (double i = h1; i <= SectionH * 2; i += 0.01)
            {
                H = i;//габаритная высота
                B = (864 - 39 * H * H * k) / (50 * H * H * k) + h1 * h1 * h1 * (39 + 50 * b1) / (50 * H * H * H);//зависимость габаритной ширины балки от высоты
                weight = GetWeight(B, H, b1, h1);//вес балки с новым сечением
                //проверка что вес балки минимален при таком сечении и что сечение корректно
                if (weight < weightMin && weight > 0)
                {
                    //вывод размеров оптимизированного сечения на экран
                    textBoxOptimH.Text = (H * 1000).ToString();
                    textBoxOptimB.Text = (B * 1000).ToString();
                    textBoxOptimWeight.Text = weight.ToString();
                    weightMin = weight;
                }
            }
        }
    }
}