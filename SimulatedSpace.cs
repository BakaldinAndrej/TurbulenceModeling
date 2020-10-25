using System;
using System.Collections.Generic;

namespace TurbulenceModeling
{
    internal class SimulatedSpace
    {
        //Точность при перерасчете давлений
        private const double Epsilon = 0.001;
        //Плотность воды в кг/м3
        private const double Density = 1000;
        //Кинематическая вязкость воды
        private const double KinematicViscosity = 0.000001006;

        /// <summary>
        /// Виды граничных условий
        /// </summary>
        public enum BoundaryCondition
        {
            /// <summary>
            /// Не является границей
            /// </summary>
            None,
            /// <summary>
            /// Парабоидальный вход
            /// </summary>
            ParabaloidInput,
            /// <summary>
            /// Простой сквозной выход
            /// </summary>
            SimpleOutput,
            /// <summary>
            /// Жесткая стенка
            /// </summary>
            SolidWall
        }

        ///<summary>
        /// Проекция скорости на ось Ox
        ///</summary>
        public double[,] VelocityX { get; set; }

        ///<summary>
        /// Проекция скорости на ось Oy
        ///</summary>
        public double[,] VelocityY { get; set; }

        ///<summary>
        /// Давление
        ///</summary>
        public double[,] Pressure { get; set; }

        /// <summary>
        /// Величина шага по сетке
        /// </summary>
        public double GridStep { get; set; }

        /// <summary>
        /// Величина шага по времени
        /// </summary>
        public double TimeStep { get; set; }

        private readonly double[,] _gradientX;
        private readonly double[,] _gradientY;

        private readonly double[,] _divergence;

        private readonly List<int> _notSimulatedSpace;

        ///<summary>
        /// Не обсчитываемая область.
        /// <para> Задается в виде листа из перечислений 4 точек ограничивающих прямоугольники </para>
        ///</summary>
        public IReadOnlyList<int> NotSimulatedSpace => _notSimulatedSpace.AsReadOnly();

        private readonly List<int> _boundaries;
        /// <summary>
        /// Список граничных условий.
        /// <para> Задается в виде листа из перечислений 4 точек ограничивающих прямоугольники и типа граничного условия <see cref="BoundaryCondition"/></para> 
        /// </summary>
        public IReadOnlyList<int> Boundaries => _boundaries.AsReadOnly();

        ///<summary>
        /// Установить начальные значения в 0
        ///</summary>
        /// <param name="xDimension"> Размерность по оси Ox </param>
        /// <param name="yDimension"> Размерность по оси Oy </param>
        /// <param name="h"> Величина шага по сетке </param>
        /// <param name="tau"> Величина шага по времени </param>
        public SimulatedSpace(int xDimension, int yDimension, double h, double tau)
        {
            VelocityX = new double[xDimension, yDimension];
            VelocityY = new double[xDimension, yDimension];
            Pressure = new double[xDimension, yDimension];
            _gradientX = new double[xDimension, yDimension];
            _gradientY = new double[xDimension, yDimension];
            _divergence = new double[xDimension, yDimension];
            GridStep = h;
            TimeStep = tau;
            _notSimulatedSpace = new List<int>(4);
            _boundaries = new List<int>(5);

            for (var i = 0; i < xDimension; i++)
            {
                for (var j = 0; j < yDimension; j++)
                {
                    VelocityX[i, j] = 0.0;
                    VelocityY[i, j] = 0.0;
                    Pressure[i, j] = 0.0;
                }
            }
        }

        ///<summary>
        /// Установить свои начальные значения
        ///</summary>
        /// <param name="velocityX"> Массив проекций скорости на ось Ox </param>
        /// <param name="velocityY"> Массив проекций скорости на ось Oy </param>
        /// <param name="pressure"> Массив проекций давления на ось Ox </param>
        /// <param name="pressureY"> Массив проекций давления на ось Oy </param>
        /// <param name="h"> Величина шага по сетке </param>
        /// <param name="tau"> Величина шага по времени </param>
        public SimulatedSpace(double[,] velocityX, double[,] velocityY, double[,] pressure, double h, double tau)
        {
            this.VelocityX = velocityX;
            this.VelocityY = velocityY;
            this.Pressure = pressure;
            _gradientX = new double[velocityX.GetLength(0), velocityX.GetLength(1)];
            _gradientY = new double[velocityX.GetLength(0), velocityX.GetLength(1)];
            _divergence = new double[velocityX.GetLength(0), velocityX.GetLength(1)];
            GridStep = h;
            TimeStep = tau;
            _notSimulatedSpace = new List<int>(4);
            _boundaries = new List<int>(5);
        }

        ///<summary>
        /// Добавить не обсчитываемую область
        ///</summary>
        /// <param name="x1"> x координата левого верхнего угла </param>
        /// <param name="y1"> y координата левого верхнего угла </param>
        /// <param name="x2"> x координата правого нижнего угла </param>
        /// <param name="y2"> y координата правого нижнего угла </param>
        public void AddNotSimulatedSpace(int x1, int y1, int x2, int y2)
        {
            _notSimulatedSpace.Add(x1);
            _notSimulatedSpace.Add(y1);
            _notSimulatedSpace.Add(x2);
            _notSimulatedSpace.Add(y2);

            for (var i = x1; i < x2; i++)
            {
                for (var j = y1; j < y2; j++)
                {
                    VelocityX[i, j] = 0.0;
                    VelocityY[i, j] = 0.0;
                    Pressure[i, j] = 0.0;
                }
            }
        }

        ///<summary>
        /// Проверка на принадлежность расчитываемой области
        ///</summary>
        /// <returns> Вернет true если элемент находится в просчитываемой области</returns>
        public bool IsInSimulatedSpace(int x, int y)
        {
            var result = true;
            for (var i = 0; i < NotSimulatedSpace.Count / 4; i++)
            {
                if (x >= NotSimulatedSpace[i * 4] && y >= NotSimulatedSpace[i * 4 + 1] &&
                    x <= NotSimulatedSpace[i * 4 + 2] && y <= NotSimulatedSpace[i * 4 + 3])
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Добавить простое граничное условие
        /// </summary>
        /// <param name="x1"> x координата левого верхнего угла </param>
        /// <param name="y1"> y координата левого верхнего угла </param>
        /// <param name="x2"> x координата правого нижнего угла </param>
        /// <param name="y2"> y координата правого нижнего угла </param>
        /// <param name="condition"> Тип граничного условия </param>
        public void AddBoundary(int x1, int y1, int x2, int y2, BoundaryCondition condition)
        {
            _boundaries.Add(x1);
            _boundaries.Add(y1);
            _boundaries.Add(x2);
            _boundaries.Add(y2);
            _boundaries.Add((int)condition);

            if (condition != BoundaryCondition.SolidWall) return;
            for (var i = x1; i <= x2; i++)
            {
                for (var j = y1; j <= y2; j++)
                {
                    VelocityX[i, j] = 0;
                    VelocityY[i, j] = 0;
                }
            }
        }

        /// <summary>
        /// Добавить парабоидальное граничное условие
        /// </summary>
        /// <param name="x1"> x координата левого верхнего угла </param>
        /// <param name="y1"> y координата левого верхнего угла </param>
        /// <param name="x2"> x координата правого нижнего угла </param>
        /// <param name="y2"> y координата правого нижнего угла </param>
        /// <param name="condition"> Тип граничного условия </param>
        /// <param name="maxVelocity"> Пиковое значение входной скорости </param>
        public void AddBoundary(int x1, int y1, int x2, int y2, BoundaryCondition condition, int maxVelocity)
        {
            _boundaries.Add(x1);
            _boundaries.Add(y1);
            _boundaries.Add(x2);
            _boundaries.Add(y2);
            _boundaries.Add((int)condition);

            for (var i = x1; i <= x2; i++)
            {
                for (var j = y1; j <= y2; j++)
                {
                    VelocityX[i, j] = x1 == x2 ? (-4 * maxVelocity * j * j) / ((y2 - y1) * (y2 - y1)) +
                                                 (4 * maxVelocity * j) / ((y2 - y1)) : 0;
                    VelocityY[i, j] = y1 == y2 ? (-4 * maxVelocity * i * i) / ((x2 - x1) * (x2 - x1)) +
                                                 (4 * maxVelocity * i) / ((x2 - x1)) : 0;
                }
            }
        }

        /// <summary>
        /// Проверка на принадлежность/тип границы
        /// </summary>
        /// <returns> Вернет тип границы если элемент находится на границе, иначе <see cref="BoundaryCondition.None"/> </returns>
        public BoundaryCondition DefineBoundaryType(int x, int y)
        {
            var result = BoundaryCondition.None;
            for (var i = 0; i < Boundaries.Count / 5; i++)
            {
                if (x >= Boundaries[i * 5] && y >= Boundaries[i * 5 + 1] &&
                    x <= Boundaries[i * 5 + 2] && y <= Boundaries[i * 5 + 3])
                {
                    result = (BoundaryCondition)Boundaries[i * 5 + 4];
                }
            }
            return result;
        }

        private void RecalculateBoundaries()
        {
            for (var i = 0; i < Boundaries.Count / 5; i += 5)
            {
                switch ((BoundaryCondition)Boundaries[i * 5 + 4])
                {
                    case BoundaryCondition.ParabaloidInput:
                        break;
                    case BoundaryCondition.None:
                        break;
                    case BoundaryCondition.SimpleOutput:
                        {
                            if (Boundaries[i * 5] == Boundaries[i * 5 + 2]) //Если сток вертикальный
                            {
                                if (Boundaries[i * 5] == 0) //Если сток слева
                                {
                                    for (var j = Boundaries[i * 5 + 1]; j < Boundaries[i * 5 + 3]; j++)
                                    {
                                        VelocityX[0, j] = VelocityX[1, j];
                                        VelocityY[0, j] = VelocityY[1, j];
                                    }
                                }
                                else //Если сток справа
                                {
                                    for (var j = Boundaries[i * 5 + 1]; j < Boundaries[i * 5 + 3]; j++)
                                    {
                                        VelocityX[VelocityX.GetLength(0), j] = VelocityX[VelocityX.GetLength(0) - 1, j];
                                        VelocityY[VelocityY.GetLength(0), j] = VelocityY[VelocityY.GetLength(0) - 1, j];
                                    }
                                }
                            }
                            else //Если сток горизонтальный
                            {
                                if (Boundaries[i * 5 + 1] == 0) //Если сток сверху
                                {
                                    for (var j = Boundaries[i * 5 + 2]; j < Boundaries[i * 5 + 4]; j++)
                                    {
                                        VelocityX[j, 0] = VelocityX[j, 1];
                                        VelocityY[j, 0] = VelocityY[j, 1];
                                    }
                                }
                                else //Если сток снизу
                                {
                                    for (var j = Boundaries[i * 5 + 2]; j < Boundaries[i * 5 + 4]; j++)
                                    {
                                        VelocityX[j, VelocityX.GetLength(1)] = VelocityX[j, VelocityX.GetLength(1) - 1];
                                        VelocityY[j, VelocityX.GetLength(1)] = VelocityY[j, VelocityX.GetLength(1) - 1];
                                    }
                                }
                            }
                            break;
                        }
                    case BoundaryCondition.SolidWall:
                        {
                            //Знак_вопроса
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void CalculateGradients()
        {
            for (var i = 0; i < VelocityX.GetLength(0); i++)
            {
                for (var j = 0; j < VelocityX.GetLength(1); j++)
                {
                    if (!IsInSimulatedSpace(i, j) || DefineBoundaryType(i, j) != BoundaryCondition.None) continue;
                    _gradientX[i, j] = VelocityX[i, j] * (VelocityX[i, j] - VelocityX[i - 1, j]) / GridStep +
                                      VelocityY[i, j] * (VelocityX[i, j] - VelocityX[i - 1, j]) / GridStep;
                    _gradientY[i, j] = VelocityX[i, j] * (VelocityY[i, j] - VelocityY[i - 1, j]) / GridStep +
                                      VelocityY[i, j] * (VelocityY[i, j] - VelocityY[i - 1, j]) / GridStep;
                }
            }
        }

        private void CalculateDivergence()
        {
            for (var i = 0; i < VelocityX.GetLength(0); i++)
            {
                for (var j = 0; j < VelocityX.GetLength(1); j++)
                {
                    if (!IsInSimulatedSpace(i, j) || DefineBoundaryType(i, j) != BoundaryCondition.None) continue;
                    _divergence[i, j] = (_gradientX[i, j] - _gradientX[i - 1, j]) / GridStep +
                                       (_gradientY[i, j] - _gradientY[i - 1, j]) / GridStep;
                }
            }
        }

        private void MakeEstablishedPressure()
        {
            var exitRule = false;
            var newPressure = Pressure;
            while (!exitRule)
            {
                exitRule = true;
                for (var i = 0; i < Pressure.GetLength(0); i++)
                {
                    for (var j = 0; j < Pressure.GetLength(1); j++)
                    {
                        if (!IsInSimulatedSpace(i, j) || DefineBoundaryType(i, j) != BoundaryCondition.None) continue;
                        newPressure[i, j] =
                            (GridStep * GridStep * Density * _divergence[i, j] +
                             Pressure[i - 1, j] + Pressure[i + 1, j] + Pressure[i, j - 1] + Pressure[i, j + 1]) / 4;
                        if (Math.Abs(newPressure[i, j] - Pressure[i, j]) > Epsilon)
                        {
                            exitRule = false;
                        }
                    }
                }

                Pressure = newPressure;
            }
        }

        /// <summary>
        /// Расчет новых скоростей
        /// </summary>
        public void CalculateNewVelocity()
        {
            CalculateGradients();
            CalculateDivergence();
            MakeEstablishedPressure();

            var newVelocityX = VelocityX;
            var newVelocityY = VelocityY;

            for (var i = 0; i < Pressure.GetLength(0); i++)
            {
                for (var j = 0; j < Pressure.GetLength(1); j++)
                {
                    if (!IsInSimulatedSpace(i, j) || DefineBoundaryType(i, j) != BoundaryCondition.None) continue;
                    newVelocityX[i, j] +=
                        TimeStep * (KinematicViscosity *
                                    (VelocityX[i - 1, j] + VelocityX[i + 1, j] + VelocityX[i, j - 1] +
                                        VelocityX[i, j + 1] - 4 * VelocityX[i, j]) / (GridStep * GridStep) -
                                    (Pressure[i, j] - Pressure[i - 1, j]) / (GridStep * Density) - _gradientX[i, j]);
                    newVelocityY[i, j] +=
                        TimeStep * (KinematicViscosity *
                                    (VelocityY[i - 1, j] + VelocityY[i + 1, j] + VelocityY[i, j - 1] +
                                        VelocityY[i, j + 1] - 4 * VelocityY[i, j]) / (GridStep * GridStep) -
                                    (Pressure[i, j] - Pressure[i, j - 1]) / (GridStep * Density) - _gradientY[i, j]);
                }
            }

            VelocityX = newVelocityX;
            VelocityY = newVelocityY;

            RecalculateBoundaries();
        }
    }
}
