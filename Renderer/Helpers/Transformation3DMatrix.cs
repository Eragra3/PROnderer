using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Helpers
{
    public class Transformation3DMatrix : Matrix
    {
        private Transformation3DMatrix()
            : base(DenseColumnMajorMatrixStorage<double>.OfValue(4, 4, 0))
        {
        }

        private Transformation3DMatrix(double[] storage)
            : base(DenseColumnMajorMatrixStorage<double>.OfColumnMajorArray(4, 4, storage))
        {
        }

        private Transformation3DMatrix(Matrix<double> matrix)
            : base(DenseColumnMajorMatrixStorage<double>.OfMatrix(matrix.Storage))
        {
        }

        public static Transformation3DMatrix GetOrthographicProjectionMatrix(CameraSettings cameraSettings)
        {
            var cs = cameraSettings;
            //column major order (column by column)
            double[] values = new double[]
            {
                2/(cs.Right-cs.Left),0,0,0,
                0,2/(cs.Top-cs.Bottom),0,0,
                0,0,2/(cs.Near-cs.Far),0,
                (cs.Right+cs.Left)/(cs.Right-cs.Left),
                (cs.Top+cs.Bottom)/(cs.Top-cs.Bottom),
                (cs.Far+cs.Near)/(cs.Far-cs.Near),
               1
            };

            //var temp = new Transformation3DMatrix(values).Transpose();
            //var data = temp.ToColumnMajorArray();

            return new Transformation3DMatrix(values);
        }

        public static Transformation3DMatrix GetPerspectiveProjectionMatrix(CameraSettings cameraSettings)
        {
            var cs = cameraSettings;
            //column major order (column by column)
            double[] values = new double[]
            {
                (2*cs.Near)/(cs.Right-cs.Left)  ,0                              ,(cs.Right+cs.Left)/(cs.Right-cs.Left)  ,0,
                0                               ,(2*cs.Near)/(cs.Top-cs.Bottom) ,(cs.Top+cs.Bottom)/(cs.Top-cs.Bottom)  ,0,
                0                               ,0                              ,(cs.Near+cs.Far)/(cs.Near-cs.Far)      ,(2*cs.Near*cs.Far)/(cs.Near-cs.Far),
                0                               ,0                              ,-1                                     ,0
            };

            //var t = new Transformation3DMatrix(values).Transpose();
            //return new Transformation3DMatrix(y);

            return new Transformation3DMatrix(values);
        }

        public static Transformation3DMatrix GetScalingMatrix(CameraSettings cameraSettings)
        {
            var cs = cameraSettings;
            //column major order (column by column)
            double[] values = new double[]
            {
                1,0,0,0,
                0,1,0,0,
                0,0,1,0,
                0,0,0,1/cameraSettings.Zoom
            };

            //var temp = new Transformation3DMatrix(values).Transpose();
            //var data = temp.ToColumnMajorArray();

            return new Transformation3DMatrix(values);
        }

        public static Transformation3DMatrix operator *(Transformation3DMatrix leftSide, Transformation3DMatrix rightSide)
        {
            if (leftSide.RowCount != 4 || leftSide.ColumnCount != 4)
            {
                throw new InvalidOperationException("Left side matrix is not 4x4");
            }
            if (rightSide.RowCount != 4 || rightSide.ColumnCount != 4)
            {
                throw new InvalidOperationException("Right side matrix is not 4x4");
            }
            Matrix<double> result = (Matrix)leftSide * rightSide;

            return new Transformation3DMatrix(result);
        }
    }
}
