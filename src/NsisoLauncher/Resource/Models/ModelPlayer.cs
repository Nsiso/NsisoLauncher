using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace NsisoLauncher.Resource.Models
{
    //public class ModelPlayer
    //{
    //    private Model3DGroup model3DGroup;
    //    private Material material;

    //    private int textureWidth = 64;
    //    private int textureHeight = 32;

    //    //** The Biped's Head
    //    private GeometryModel3D bipedHead;
    //    private GeometryModel3D bipedHeadwear;
    //    // private GeometryModel3D bipedDeadmau5Head;

    //    // The Biped's Headwear. Used for the outer layer of player skins.
    //    private GeometryModel3D bipedBody;
    //    private GeometryModel3D bipedBodyWear;

    //    // The Biped's Right Arm
    //    private GeometryModel3D bipedRightArm;
    //    private GeometryModel3D bipedRightArmwear;

    //    // The Biped's Left Arm
    //    private GeometryModel3D bipedLeftArm;
    //    private GeometryModel3D bipedLeftArmwear;

    //    // The Biped's Right Leg
    //    private GeometryModel3D bipedRightLeg;
    //    private GeometryModel3D bipedRightLegwear;

    //    // The Biped's Left Leg
    //    private GeometryModel3D bipedLeftLeg;
    //    private GeometryModel3D bipedLeftLegwear;

    //    // Cape
    //    private GeometryModel3D bipedCape;

    //    public bool isSneak;
    //    private bool smallArms;

    //    //public ModelPlayer(double modelSize, bool smallArmsIn, int textureWidthIn, int textureHeightIn)
    //    //{
    //    //    this.textureWidth = textureWidthIn;
    //    //    this.textureHeight = textureHeightIn;

    //    //    this.bipedHead = new GeometryModel3D()
    //    //    {
    //    //        Geometry = new MeshGeometry3D()
    //    //        {
    //    //            Positions = GetBox(-4.0, -8.0, -4.0, 8, 8, 8, modelSize),
    //    //            TextureCoordinates = new PointCollection(),

    //    //        },
    //    //        Transform = new MatrixTransform3D()
    //    //        {
    //    //            d
    //    //        }
    //    //    };


    //    //}

    //    private Point3DCollection GetBox(double offX, double offY, double offZ, int width, int height, int depth, double scaleFactor)
    //    {
    //        double f = offX + width + scaleFactor;
    //        double f1 = offY + height + scaleFactor;
    //        double f2 = offZ + depth + scaleFactor;

    //        double x = offX - scaleFactor;
    //        double y = offY - scaleFactor;
    //        double z = offZ - scaleFactor;

    //        Point3DCollection points = new Point3DCollection();
    //        points.Add(new Point3D(x, y, z));
    //        points.Add(new Point3D(f, y, z));
    //        points.Add(new Point3D(f, f1, z));
    //        points.Add(new Point3D(x, f1, z));
    //        points.Add(new Point3D(x, y, f2));
    //        points.Add(new Point3D(f, y, f2));
    //        points.Add(new Point3D(f, f1, f2));
    //        points.Add(new Point3D(x, f1, f2));

    //        return points;
    //    }
    //}
}
