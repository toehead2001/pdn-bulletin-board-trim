using System;
using System.Drawing;
using System.Reflection;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

namespace BulletinBoardTrimEffect
{
    public class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author
        {
            get
            {
                return ((AssemblyCopyrightAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
            }
        }
        public string Copyright
        {
            get
            {
                return ((AssemblyDescriptionAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0]).Description;
            }
        }

        public string DisplayName
        {
            get
            {
                return ((AssemblyProductAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product;
            }
        }

        public Version Version
        {
            get
            {
                return base.GetType().Assembly.GetName().Version;
            }
        }

        public Uri WebsiteUri
        {
            get
            {
                return new Uri("http://www.getpaint.net/redirect/plugins.html");
            }
        }
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Bulletin Board Trim")]
    public class BulletinBoardTrimEffectPlugin : PropertyBasedEffect
    {
        public static string StaticName
        {
            get
            {
                return "Bulletin Board Trim";
            }
        }

        public static Image StaticIcon
        {
            get
            {
                return new Bitmap(typeof(BulletinBoardTrimEffectPlugin), "BulletinBoardTrim.png");
            }
        }

        public static string SubmenuName
        {
            get
            {
                return SubmenuNames.Render;
            }
        }

        public BulletinBoardTrimEffectPlugin()
            : base(StaticName, StaticIcon, SubmenuName, EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Amount1,
            Amount2,
            Amount3,
            Amount4,
            Amount5,
            Amount6,
            Amount7,
            Amount8,
            Amount9,
            Amount10
        }

        public enum Amount2Options
        {
            Amount2Option1,
            Amount2Option2,
            Amount2Option3,
            Amount2Option4,
            Amount2Option5,
            Amount2Option6,
            Amount2Option7,
            Amount2Option8
        }


        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            ColorBgra PrimaryColor = EnvironmentParameters.PrimaryColor;
            PrimaryColor.A = 255;
            ColorBgra SecondaryColor = EnvironmentParameters.SecondaryColor;
            SecondaryColor.A = 255;

            props.Add(new Int32Property(PropertyNames.Amount1, 50, 0, 100));
            props.Add(StaticListChoiceProperty.CreateForEnum<Amount2Options>(PropertyNames.Amount2, 0, false));
            props.Add(new Int32Property(PropertyNames.Amount3, 60, 5, 100));
            props.Add(new Int32Property(PropertyNames.Amount4, 20, 5, 100));
            props.Add(new Int32Property(PropertyNames.Amount5, ColorBgra.ToOpaqueInt32(PrimaryColor), 0, 0xffffff));
            props.Add(new BooleanProperty(PropertyNames.Amount6, false));
            props.Add(new BooleanProperty(PropertyNames.Amount7, false));
            props.Add(new Int32Property(PropertyNames.Amount8, 2, 2, 10));
            props.Add(new Int32Property(PropertyNames.Amount9, ColorBgra.ToOpaqueInt32(SecondaryColor), 0, 0xffffff));
            props.Add(new Int32Property(PropertyNames.Amount10, 255, 0, 255));

            List<PropertyCollectionRule> propRules = new List<PropertyCollectionRule>();
            propRules.Add(new ReadOnlyBoundToBooleanRule(PropertyNames.Amount7, PropertyNames.Amount6, true));
            propRules.Add(new ReadOnlyBoundToBooleanRule(PropertyNames.Amount8, PropertyNames.Amount6, true));
            propRules.Add(new ReadOnlyBoundToBooleanRule(PropertyNames.Amount9, PropertyNames.Amount6, true));

            return new PropertyCollection(props, propRules);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Amount1, ControlInfoPropertyNames.DisplayName, "Trim Size");
            configUI.SetPropertyControlValue(PropertyNames.Amount2, ControlInfoPropertyNames.DisplayName, "Hump Style");
            PropertyControlInfo Amount2Control = configUI.FindControlForPropertyName(PropertyNames.Amount2);
            Amount2Control.SetValueDisplayName(Amount2Options.Amount2Option1, "Round Wave");
            Amount2Control.SetValueDisplayName(Amount2Options.Amount2Option2, "Pointy Wave");
            Amount2Control.SetValueDisplayName(Amount2Options.Amount2Option3, "Square Wave");
            Amount2Control.SetValueDisplayName(Amount2Options.Amount2Option4, "Wishbone Wave");
            Amount2Control.SetValueDisplayName(Amount2Options.Amount2Option5, "Pointy / Round (out)");
            Amount2Control.SetValueDisplayName(Amount2Options.Amount2Option6, "Pointy / Round (in)");
            Amount2Control.SetValueDisplayName(Amount2Options.Amount2Option7, "Pointy & Round (in)");
            Amount2Control.SetValueDisplayName(Amount2Options.Amount2Option8, "Pointy & Round (out)");
            configUI.SetPropertyControlValue(PropertyNames.Amount3, ControlInfoPropertyNames.DisplayName, "Hump Gap");
            configUI.SetPropertyControlValue(PropertyNames.Amount4, ControlInfoPropertyNames.DisplayName, "Hump Protrusion");
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.DisplayName, "Trim Color");
            configUI.SetPropertyControlType(PropertyNames.Amount5, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.DisplayName, "Trim Border");
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.Description, "Draw Border");
            configUI.SetPropertyControlValue(PropertyNames.Amount7, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.Amount7, ControlInfoPropertyNames.Description, "Overlap Border in Corners");
            configUI.SetPropertyControlValue(PropertyNames.Amount8, ControlInfoPropertyNames.DisplayName, "Border Width");
            configUI.SetPropertyControlValue(PropertyNames.Amount9, ControlInfoPropertyNames.DisplayName, "Border Color");
            configUI.SetPropertyControlType(PropertyNames.Amount9, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.Amount10, ControlInfoPropertyNames.DisplayName, "Trim Opacity");
            configUI.SetPropertyControlValue(PropertyNames.Amount10, ControlInfoPropertyNames.ControlColors, new ColorBgra[] { ColorBgra.White, ColorBgra.Black });

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Amount1 = newToken.GetProperty<Int32Property>(PropertyNames.Amount1).Value;
            Amount2 = (byte)((int)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.Amount2).Value);
            Amount3 = newToken.GetProperty<Int32Property>(PropertyNames.Amount3).Value;
            Amount4 = newToken.GetProperty<Int32Property>(PropertyNames.Amount4).Value;
            Amount5 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount5).Value);
            Amount6 = newToken.GetProperty<BooleanProperty>(PropertyNames.Amount6).Value;
            Amount7 = newToken.GetProperty<BooleanProperty>(PropertyNames.Amount7).Value;
            Amount8 = newToken.GetProperty<Int32Property>(PropertyNames.Amount8).Value;
            Amount9 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount9).Value);
            Amount10 = newToken.GetProperty<Int32Property>(PropertyNames.Amount10).Value;


            Rectangle selection = EnvironmentParameters.GetSelection(srcArgs.Surface.Bounds).GetBoundsInt();
            int centerX = ((selection.Right - selection.Left) / 2) + selection.Left;
            int centerY = ((selection.Bottom - selection.Top) / 2) + selection.Top;
            int offsetX = centerX - selection.Width / Amount3 / 2 * Amount3;
            int offsetY = centerY - selection.Height / Amount3 / 2 * Amount3;
            float trimSize = Amount1 - Amount4 - (Amount6 ? Amount8 : 0);
            float wave;

            // Horizontal Points
            PointF[] topPoints = new PointF[selection.Width + 2];
            PointF[] bottomPoints = new PointF[selection.Width + 2];

            topPoints[0] = new PointF(selection.Left - 50, selection.Top - 50);
            bottomPoints[0] = new PointF(selection.Left - 50, selection.Bottom + 50);
            for (int i = 0; i < selection.Width; i++)
            {
                wave = trimSize + getEquation(i - offsetX);

                topPoints[i + 1] = new PointF(selection.Left + i, selection.Top + wave);
                bottomPoints[i + 1] = new PointF(selection.Left + i, selection.Bottom - wave);
            }
            topPoints[selection.Width + 1] = new PointF(selection.Right + 10, selection.Top - 10);
            bottomPoints[selection.Width + 1] = new PointF(selection.Right + 100, selection.Bottom + 100);


            // Vertical Points
            PointF[] rightPoints = new PointF[selection.Height + 2];
            PointF[] leftPoints = new PointF[selection.Height + 2];

            rightPoints[0] = new PointF(selection.Right + 50, selection.Top - 50);
            leftPoints[0] = new PointF(selection.Left - 50, selection.Top - 50);
            for (int i = 0; i < selection.Height; i++)
            {
                wave = trimSize + getEquation(i - offsetY);

                rightPoints[i + 1] = new PointF(selection.Right - wave, selection.Top + i);
                leftPoints[i + 1] = new PointF(selection.Left + wave, selection.Top + i);
            }
            rightPoints[selection.Height + 1] = new PointF(selection.Right + 100, selection.Bottom + 100);
            leftPoints[selection.Height + 1] = new PointF(selection.Left - 100, selection.Bottom + 100);


            if (trimSurface == null)
                trimSurface = new Surface(srcArgs.Surface.Size);
            else
                trimSurface.Clear(Color.Transparent);

            using (RenderArgs ra = new RenderArgs(trimSurface))
            {
                Graphics board = ra.Graphics;
                board.SmoothingMode = SmoothingMode.AntiAlias;

                using (SolidBrush trimBrush = new SolidBrush(Amount5))
                using (Pen borderPen = new Pen(Amount9, Amount8 * 2))
                {
                    if (Amount7 && Amount6) // Border & Overlap
                    {
                        board.DrawLines(borderPen, topPoints);
                        board.FillPolygon(trimBrush, topPoints);

                        board.DrawLines(borderPen, rightPoints);
                        board.FillPolygon(trimBrush, rightPoints);

                        board.DrawLines(borderPen, bottomPoints);
                        board.FillPolygon(trimBrush, bottomPoints);

                        board.DrawLines(borderPen, leftPoints);
                        board.FillPolygon(trimBrush, leftPoints);

                        // Top Left hackfix
                        PointF[] fixPoints = new PointF[Amount1 + Amount8 + 1];
                        Array.Copy(topPoints, fixPoints, fixPoints.Length);
                        board.DrawLines(borderPen, fixPoints);
                        Array.Resize(ref topPoints, Amount1 + Amount8 + 20);
                        topPoints[Amount1 + Amount8 + 19] = new PointF(selection.Left + Amount1 + Amount8 + 17, selection.Top - 50);
                        board.FillPolygon(trimBrush, topPoints);
                    }
                    else if (Amount6) // Border only
                    {
                        board.DrawLines(borderPen, topPoints);
                        board.DrawLines(borderPen, rightPoints);
                        board.DrawLines(borderPen, bottomPoints);
                        board.DrawLines(borderPen, leftPoints);

                        board.FillPolygon(trimBrush, topPoints);
                        board.FillPolygon(trimBrush, rightPoints);
                        board.FillPolygon(trimBrush, bottomPoints);
                        board.FillPolygon(trimBrush, leftPoints);
                    }
                    else // no Border
                    {
                        board.FillPolygon(trimBrush, topPoints);
                        board.FillPolygon(trimBrush, rightPoints);
                        board.FillPolygon(trimBrush, bottomPoints);
                        board.FillPolygon(trimBrush, leftPoints);
                    }

                }
            }


            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            if (length == 0) return;
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Render(DstArgs.Surface, SrcArgs.Surface, renderRects[i]);
            }
        }


        int Amount1 = 50; // [0,100] Trim Size
        byte Amount2 = 0; // Hump Style|Round Wave|Pointy Wave|Square Wave|Wishbone Wave|Pointy / Round (out)|Pointy / Round (in)|Pointy & Round (in)|Pointy & Round (out)
        int Amount3 = 60; // [5,100] Hump Gap
        int Amount4 = 20; // [5,100] Hump Protrusion
        ColorBgra Amount5 = ColorBgra.FromBgr(0, 0, 0); // [PrimaryColor] Trim Color
        bool Amount6 = false; // [0,1] Border
        bool Amount7 = false; // [0,1] Overlap in Corners
        int Amount8 = 2; // [2,10] Border Width
        ColorBgra Amount9 = ColorBgra.FromBgr(255, 0, 255); // [SecondaryColor] Border Color
        int Amount10 = 255; // [0,255] Trim Opacity

        BinaryPixelOp normalOp = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal);
        Surface trimSurface;

        void Render(Surface dst, Surface src, Rectangle rect)
        {
            ColorBgra sourcePixel, trimPixel;
            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                if (IsCancelRequested) return;
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    sourcePixel = src[x, y];
                    trimPixel = trimSurface[x, y];
                    trimPixel.A = Int32Util.ClampToByte(trimPixel.A * Amount10 / 255);
                    dst[x, y] = normalOp.Apply(sourcePixel, trimPixel);
                }
            }
        }

        float getEquation(int i)
        {
            i = Math.Abs(i);

            float equation = 0;
            switch (Amount2)
            {
                case 0: // Cosine Wave
                    equation = (float)(Amount4 / 2f * Math.Cos(Math.PI * i / (Amount3 / 2f))) + Amount4 / 2f;
                    break;
                case 1: // Pointy Wave
                    equation = (float)((Amount4 / Math.PI) * Math.Acos(Math.Cos(2 * (Math.PI / Amount3) * (i + Amount3 / 2f))));
                    break;
                case 2: // Square Wave
                    equation = (Math.Sin(Math.PI * (i + Amount3 / 4f) / (Amount3 / 2f)) >= 0) ? Amount4 : 0;
                    break;
                case 3: // Wish Bone
                    equation = ((int)((i + Amount3 / 4f) / (Amount3 / 2f)) % 2 != 0) ?
                                    (float)(Amount4 / 2f * Math.Abs(Math.Cos(Math.PI * (i + Amount3 / 4f) / (Amount3 / 2f)))) :
                                    (float)(Amount4 / 2f * -Math.Abs(Math.Cos(Math.PI * (i + Amount3 / 4f) / (Amount3 / 2f)))) + Amount4;
                    break;
                case 4: // Pointy / Round (out)
                    equation = (float)(Amount4 * -Math.Abs(Math.Sin(Math.PI * i / Amount3))) + Amount4;
                    break;
                case 5: // Pointy / Round (in)
                    equation = (float)(Amount4 * Math.Abs(Math.Cos(Math.PI * i / Amount3)));
                    break;
                case 6: // Pointy & Round (in)
                    equation = ((int)((i + Amount3 / 2f) / Amount3) % 2 != 0) ?
                                    (float)(Amount4 * Math.Abs(Math.Cos(Math.PI * (i + Amount3) / Amount3))) :
                                    (float)(Amount4 * -Math.Abs(Math.Cos(Math.PI * (i + Amount3 / 2f) / Amount3))) + Amount4;
                    break;
                case 7: // Pointy & Round (out)
                    equation = ((int)((i + Amount3 / 2f) / Amount3) % 2 != 0) ?
                                    (float)(Amount4 * -Math.Abs(Math.Cos(Math.PI * (i + Amount3) / Amount3))) + Amount4 :
                                    (float)(Amount4 * Math.Abs(Math.Cos(Math.PI * (i + Amount3 / 2f) / Amount3)));
                    break;
            }
            return equation;
        }

    }
}
