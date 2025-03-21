using Autodesk.Revit.DB;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.CommonUtils
{
    public static class ModelCurveUtils
    {
        /// <summary>
        /// 创建模型线
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="line"></param>
        public static ModelCurve CreateModelLine(Document doc, Line line)
        {
            XYZ normal = line.Direction.CrossProduct(XYZ.BasisX);
           
            normal = normal.IsAlmostEqualTo(XYZ.Zero) ? line.Direction.CrossProduct(XYZ.BasisY) : normal;
            normal = normal.IsAlmostEqualTo(XYZ.Zero) ? line.Direction.CrossProduct(XYZ.BasisZ) : normal;

            Plane plane = Plane.CreateByNormalAndOrigin(normal, line.GetEndPoint(0));
            var modelCurve = doc.Create.NewModelCurve(line, SketchPlane.Create(doc, plane));
            return modelCurve;
        }

        public static ModelCurve CreateModelCurve(Document doc, Curve curve)
        {
            XYZ normal = XYZ.BasisZ;

            Plane plane = Plane.CreateByNormalAndOrigin(normal, curve.GetEndPoint(0));
            var sketch = SketchPlane.Create(doc, plane);
            var modelCurve = doc.Create.NewModelCurve(curve, sketch);
            return modelCurve;
        }

        public static ModelCurve CreateModelCurve(Document doc, Arc arc)
        {
            Plane plane = Plane.CreateByNormalAndOrigin(arc.Normal, arc.Center);
            SketchPlane sketch = SketchPlane.Create(doc, plane);
            ModelCurve modelCurve = doc.Create.NewModelCurve(arc, sketch);
            return modelCurve;
        }

        /// <summary>
        /// 创建模型线圆
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="centerPoint"></param>
        /// <returns></returns>
        public static ModelCurve CreateModelArc(Document doc, XYZ centerPoint, double r)
        {
            XYZ normal = new XYZ(0, 0, 1);
            double radius = r / 304.8;
            double startAngle = 0;
            double endAngle = 2 * Math.PI;

            Plane plane = Plane.CreateByNormalAndOrigin(normal, centerPoint);
            Arc arc = Arc.Create(plane, radius, startAngle, endAngle);
            SketchPlane sketch = SketchPlane.Create(doc, plane);
            ModelCurve modelCurve = doc.Create.NewModelCurve(arc, sketch);
            return modelCurve;
        }
    }
}
