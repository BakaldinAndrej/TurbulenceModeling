using System;
using System.Drawing;
using System.Windows.Forms;

namespace TurbulenceModeling
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Ширина отображаемой картинки в px
        /// </summary>
        private const int BmpWidth = 1000;
        /// <summary>
        /// Высота отображаемой картинки в px
        /// </summary>
        private const int BmpHeight = 500;
        /// <summary>
        /// Число шагов разбиения пространства моделирования по ширине
        /// </summary>
        private const int SpaceWidth = 1000;
        /// <summary>
        /// Число шагов разбиения пространства моделирования по высоте
        /// </summary>
        private const int SpaceHeight = 500;
        /// <summary>
        /// Константа для скалирования изображения
        /// </summary>
        private const int SpaceBmpRatio = SpaceWidth / BmpWidth;

        private readonly Random _rnd = new Random();
        private static readonly Bitmap Bmp = new Bitmap(BmpWidth + 200, BmpHeight + 200);
        private readonly Graphics _graph = Graphics.FromImage(Bmp);
        private readonly Pen _pen = new Pen(Color.Black, 1);

        private SimulatedSpace _space;

        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Отрисовка текущего моделируемого пространства
        /// </summary>
        private void Render()
        {
            #region Нормализация

            var maxVelocity = Math.Abs(_space.VelocityX[0, 0]);
            var minVelocity = maxVelocity;

            for (var i = 0; i < SpaceWidth; i++)
            {
                for (var j = 0; j < SpaceHeight; j++)
                {
                    if (maxVelocity < Math.Abs(_space.VelocityX[i, j]))
                    {
                        maxVelocity = Math.Abs(_space.VelocityX[i, j]);
                    }
                    if (maxVelocity < Math.Abs(_space.VelocityY[i, j]))
                    {
                        maxVelocity = Math.Abs(_space.VelocityY[i, j]);
                    }
                    if (minVelocity > Math.Abs(_space.VelocityX[i, j]))
                    {
                        minVelocity = Math.Abs(_space.VelocityX[i, j]);
                    }
                    if (minVelocity > Math.Abs(_space.VelocityY[i, j]))
                    {
                        minVelocity = Math.Abs(_space.VelocityY[i, j]);
                    }
                }
            }

            var normalizedVelocityX = new double[SpaceWidth, SpaceHeight];
            var normalizedVelocityY = new double[SpaceWidth, SpaceHeight];
            var maxMinRatio = maxVelocity - minVelocity;

            for (var i = 0; i < SpaceWidth; i++)
            {
                for (var j = 0; j < SpaceHeight; j++)
                {
                    if (_space.IsInSimulatedSpace(i, j))
                    {
                        normalizedVelocityX[i, j] = (_space.VelocityX[i, j] - minVelocity) / maxMinRatio;
                        normalizedVelocityY[i, j] = (_space.VelocityY[i, j] - minVelocity) / maxMinRatio;
                    }
                    else
                    {
                        normalizedVelocityX[i, j] = normalizedVelocityY[i, j] = 0.0;
                    }
                }
            }

            #endregion

            #region Отрисовка скоростей

            _pen.Width = 1;
            for (var i = 0; i < SpaceWidth; i += /*10 **/ SpaceBmpRatio)
            {
                for (var j = 0; j < SpaceHeight; j += /*10 **/ SpaceBmpRatio)
                {
                    if (!_space.IsInSimulatedSpace(i, j)) continue;
                    _graph.DrawLine(_pen, i / SpaceBmpRatio + 10, j / SpaceBmpRatio + 10,
                        Convert.ToInt32(i / SpaceBmpRatio + normalizedVelocityX[i, j] * 8) + 10,
                        Convert.ToInt32(j / SpaceBmpRatio + normalizedVelocityY[i, j] * 8) + 10);
                }
            }

            #endregion

            #region Отрисовка границ и не обсчитываемых областей

            _pen.Width = 4;
            _graph.DrawRectangle(_pen, 10, 10, SpaceWidth / SpaceBmpRatio, SpaceHeight / SpaceBmpRatio);
            for (var i = 0; i < _space.NotSimulatedSpace.Count / 4; i++)
            {
                _graph.FillRectangle(Brushes.Black, _space.NotSimulatedSpace[i * 4] / SpaceBmpRatio + 10, _space.NotSimulatedSpace[i * 4 + 1] / SpaceBmpRatio + 10,
                    _space.NotSimulatedSpace[i * 4 + 2] / SpaceBmpRatio - _space.NotSimulatedSpace[i * 4] / SpaceBmpRatio,
                    _space.NotSimulatedSpace[i * 4 + 3] / SpaceBmpRatio - _space.NotSimulatedSpace[i * 4 + 1] / SpaceBmpRatio);
            }

            #endregion

            _pctbMain.Image = Bmp;
        }

        private void StartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            _space = new SimulatedSpace(SpaceWidth, SpaceHeight, 0.01, 0.1);

            //Задание рандомных скоростей
            //for (var i = 0; i < SpaceWidth; i++)
            //{
            //    for (var j = 0; j < SpaceHeight; j++)
            //    {
            //        _space.VelocityX[i, j] = _rnd.NextDouble() / 0.6 - 0.8;
            //        _space.VelocityY[i, j] = _rnd.NextDouble() / 0.6 - 0.8;
            //    }
            //}

            //Добавление не обсчитываемых областей
            _space.AddNotSimulatedSpace(0 * SpaceBmpRatio, 300 * SpaceBmpRatio, 350 * SpaceBmpRatio, 500 * SpaceBmpRatio);
            _space.AddNotSimulatedSpace(650 * SpaceBmpRatio, 300 * SpaceBmpRatio, 1000 * SpaceBmpRatio, 500 * SpaceBmpRatio);

            //Добавление граничных условий
            _space.AddBoundary(0, 0, 0, 299, SimulatedSpace.BoundaryCondition.ParabaloidInput, 10);
            _space.AddBoundary(999, 0, 999, 299, SimulatedSpace.BoundaryCondition.SimpleOutput);
            _space.AddBoundary(0, 0, 999, 0, SimulatedSpace.BoundaryCondition.SolidWall);
            _space.AddBoundary(0, 299, 349, 299, SimulatedSpace.BoundaryCondition.SolidWall);
            _space.AddBoundary(650, 299, 999, 299, SimulatedSpace.BoundaryCondition.SolidWall);
            _space.AddBoundary(350, 499, 649, 499, SimulatedSpace.BoundaryCondition.SolidWall);
            _space.AddBoundary(350, 300, 350, 499, SimulatedSpace.BoundaryCondition.SolidWall);
            _space.AddBoundary(650, 300, 650, 499, SimulatedSpace.BoundaryCondition.SolidWall);

            for (var i = 0; i < 5; i++)
            {
                _space.CalculateNewVelocity();
            }
            Render();

            Cursor.Current = Cursors.Arrow;
        }
    }
}
