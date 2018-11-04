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
        public string Author => base.GetType().Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
        public string Copyright => base.GetType().Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
        public string DisplayName => base.GetType().Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        public Version Version => base.GetType().Assembly.GetName().Version;
        public Uri WebsiteUri => new Uri("https://forums.getpaint.net/index.php?showtopic=109942");
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Bulletin Board Trim")]
    public class BulletinBoardTrimEffectPlugin : PropertyBasedEffect
    {
        private static readonly Image StaticIcon = new Bitmap(typeof(BulletinBoardTrimEffectPlugin), "BulletinBoardTrim.png");

        public BulletinBoardTrimEffectPlugin()
            : base("Bulletin Board Trim", StaticIcon, SubmenuNames.Render, EffectFlags.Configurable)
        {
        }

        private enum PropertyNames
        {
            TrimSize,
            HumpStyle,
            HumpGap,
            HumpProtrusion,
            TrimColor,
            TrimBorder,
            OverlapBorder,
            BorderWidth,
            BorderColor,
            TrimOpactiy
        }

        private enum HumpStyle
        {
            RoundWave,
            PointyWave,
            SquareWave,
            WishboneWave,
            PointyRound1Out,
            PointyRound1In,
            PointyRound2In,
            PointyRound2Out
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            ColorBgra primaryColor = EnvironmentParameters.PrimaryColor.NewAlpha(255);
            ColorBgra secondaryColor = EnvironmentParameters.SecondaryColor.NewAlpha(255);

            List<Property> props = new List<Property>
            {
                new Int32Property(PropertyNames.TrimSize, 50, 0, 100),
                StaticListChoiceProperty.CreateForEnum<HumpStyle>(PropertyNames.HumpStyle, 0, false),
                new Int32Property(PropertyNames.HumpGap, 60, 5, 100),
                new Int32Property(PropertyNames.HumpProtrusion, 20, 5, 100),
                new Int32Property(PropertyNames.TrimColor, ColorBgra.ToOpaqueInt32(primaryColor), 0, 0xffffff),
                new BooleanProperty(PropertyNames.TrimBorder, false),
                new BooleanProperty(PropertyNames.OverlapBorder, false),
                new Int32Property(PropertyNames.BorderWidth, 2, 2, 10),
                new Int32Property(PropertyNames.BorderColor, ColorBgra.ToOpaqueInt32(secondaryColor), 0, 0xffffff),
                new Int32Property(PropertyNames.TrimOpactiy, 255, 0, 255)
            };

            List<PropertyCollectionRule> propRules = new List<PropertyCollectionRule>
            {
                new ReadOnlyBoundToBooleanRule(PropertyNames.OverlapBorder, PropertyNames.TrimBorder, true),
                new ReadOnlyBoundToBooleanRule(PropertyNames.BorderWidth, PropertyNames.TrimBorder, true),
                new ReadOnlyBoundToBooleanRule(PropertyNames.BorderColor, PropertyNames.TrimBorder, true)
            };

            return new PropertyCollection(props, propRules);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.TrimSize, ControlInfoPropertyNames.DisplayName, "Trim Size");
            configUI.SetPropertyControlValue(PropertyNames.HumpStyle, ControlInfoPropertyNames.DisplayName, "Hump Style");
            PropertyControlInfo humpStyleControl = configUI.FindControlForPropertyName(PropertyNames.HumpStyle);
            humpStyleControl.SetValueDisplayName(HumpStyle.RoundWave, "Round Wave");
            humpStyleControl.SetValueDisplayName(HumpStyle.PointyWave, "Pointy Wave");
            humpStyleControl.SetValueDisplayName(HumpStyle.SquareWave, "Square Wave");
            humpStyleControl.SetValueDisplayName(HumpStyle.WishboneWave, "Wishbone Wave");
            humpStyleControl.SetValueDisplayName(HumpStyle.PointyRound1Out, "Pointy / Round (out)");
            humpStyleControl.SetValueDisplayName(HumpStyle.PointyRound1In, "Pointy / Round (in)");
            humpStyleControl.SetValueDisplayName(HumpStyle.PointyRound2In, "Pointy & Round (in)");
            humpStyleControl.SetValueDisplayName(HumpStyle.PointyRound2Out, "Pointy & Round (out)");
            configUI.SetPropertyControlValue(PropertyNames.HumpGap, ControlInfoPropertyNames.DisplayName, "Hump Gap");
            configUI.SetPropertyControlValue(PropertyNames.HumpProtrusion, ControlInfoPropertyNames.DisplayName, "Hump Protrusion");
            configUI.SetPropertyControlValue(PropertyNames.TrimColor, ControlInfoPropertyNames.DisplayName, "Trim Color");
            configUI.SetPropertyControlType(PropertyNames.TrimColor, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.TrimBorder, ControlInfoPropertyNames.DisplayName, "Trim Border");
            configUI.SetPropertyControlValue(PropertyNames.TrimBorder, ControlInfoPropertyNames.Description, "Draw Border");
            configUI.SetPropertyControlValue(PropertyNames.OverlapBorder, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.OverlapBorder, ControlInfoPropertyNames.Description, "Overlap Border in Corners");
            configUI.SetPropertyControlValue(PropertyNames.BorderWidth, ControlInfoPropertyNames.DisplayName, "Border Width");
            configUI.SetPropertyControlValue(PropertyNames.BorderColor, ControlInfoPropertyNames.DisplayName, "Border Color");
            configUI.SetPropertyControlType(PropertyNames.BorderColor, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.TrimOpactiy, ControlInfoPropertyNames.DisplayName, "Trim Opacity");
            configUI.SetPropertyControlValue(PropertyNames.TrimOpactiy, ControlInfoPropertyNames.ControlColors, new ColorBgra[] { ColorBgra.White, ColorBgra.Black });

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            size = newToken.GetProperty<Int32Property>(PropertyNames.TrimSize).Value;
            humpStyle = (HumpStyle)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.HumpStyle).Value;
            humpGap = newToken.GetProperty<Int32Property>(PropertyNames.HumpGap).Value;
            humpProtru = newToken.GetProperty<Int32Property>(PropertyNames.HumpProtrusion).Value;
            trimColor = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.TrimColor).Value);
            trimBorder = newToken.GetProperty<BooleanProperty>(PropertyNames.TrimBorder).Value;
            overlapBorder = newToken.GetProperty<BooleanProperty>(PropertyNames.OverlapBorder).Value;
            borderWidth = newToken.GetProperty<Int32Property>(PropertyNames.BorderWidth).Value;
            borderColor = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.BorderColor).Value);
            trimOpacity = newToken.GetProperty<Int32Property>(PropertyNames.TrimOpactiy).Value;

            Rectangle selection = EnvironmentParameters.GetSelection(srcArgs.Surface.Bounds).GetBoundsInt();
            int centerX = ((selection.Right - selection.Left) / 2) + selection.Left;
            int centerY = ((selection.Bottom - selection.Top) / 2) + selection.Top;
            int offsetX = centerX - selection.Width / humpGap / 2 * humpGap;
            int offsetY = centerY - selection.Height / humpGap / 2 * humpGap;
            float trimSize = size - humpProtru - (trimBorder ? borderWidth : 0);
            float wave;

            // Horizontal Points
            PointF[] topPoints = new PointF[selection.Width + 2];
            PointF[] bottomPoints = new PointF[selection.Width + 2];

            topPoints[0] = new PointF(selection.Left - 50, selection.Top - 50);
            bottomPoints[0] = new PointF(selection.Left - 50, selection.Bottom + 50);
            for (int i = 0; i < selection.Width; i++)
            {
                wave = trimSize + GetEquation(i - offsetX);

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
                wave = trimSize + GetEquation(i - offsetY);

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

                using (SolidBrush trimBrush = new SolidBrush(trimColor))
                using (Pen borderPen = new Pen(borderColor, borderWidth * 2))
                {
                    if (overlapBorder && trimBorder) // Border & Overlap
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
                        PointF[] fixPoints = new PointF[size + borderWidth + 1];
                        Array.Copy(topPoints, fixPoints, fixPoints.Length);
                        board.DrawLines(borderPen, fixPoints);
                        Array.Resize(ref topPoints, size + borderWidth + 20);
                        topPoints[size + borderWidth + 19] = new PointF(selection.Left + size + borderWidth + 17, selection.Top - 50);
                        board.FillPolygon(trimBrush, topPoints);
                    }
                    else if (trimBorder) // Border only
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

        private int size = 50; // [0,100] Trim Size
        private HumpStyle humpStyle = 0; // Hump Style|Round Wave|Pointy Wave|Square Wave|Wishbone Wave|Pointy / Round (out)|Pointy / Round (in)|Pointy & Round (in)|Pointy & Round (out)
        private int humpGap = 60; // [5,100] Hump Gap
        private int humpProtru = 20; // [5,100] Hump Protrusion
        private ColorBgra trimColor = ColorBgra.FromBgr(0, 0, 0); // [PrimaryColor] Trim Color
        private bool trimBorder = false; // [0,1] Border
        private bool overlapBorder = false; // [0,1] Overlap in Corners
        private int borderWidth = 2; // [2,10] Border Width
        private ColorBgra borderColor = ColorBgra.FromBgr(255, 0, 255); // [SecondaryColor] Border Color
        private int trimOpacity = 255; // [0,255] Trim Opacity

        private readonly BinaryPixelOp normalOp = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal);
        private Surface trimSurface;

        private void Render(Surface dst, Surface src, Rectangle rect)
        {
            ColorBgra sourcePixel, trimPixel;
            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                if (IsCancelRequested) return;
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    sourcePixel = src[x, y];
                    trimPixel = trimSurface[x, y];
                    trimPixel.A = Int32Util.ClampToByte(trimPixel.A * trimOpacity / 255);
                    dst[x, y] = normalOp.Apply(sourcePixel, trimPixel);
                }
            }
        }

        private float GetEquation(int i)
        {
            i = Math.Abs(i);
            switch (humpStyle)
            {
                case HumpStyle.RoundWave: // Cosine Wave
                    return (float)(humpProtru / 2f * Math.Cos(Math.PI * i / (humpGap / 2f))) + humpProtru / 2f;
                case HumpStyle.PointyWave: // Pointy Wave
                    return (float)((humpProtru / Math.PI) * Math.Acos(Math.Cos(2 * (Math.PI / humpGap) * (i + humpGap / 2f))));
                case HumpStyle.SquareWave: // Square Wave
                    return (Math.Sin(Math.PI * (i + humpGap / 4f) / (humpGap / 2f)) >= 0) ? humpProtru : 0;
                case HumpStyle.WishboneWave: // Wish Bone
                    return ((int)((i + humpGap / 4f) / (humpGap / 2f)) % 2 != 0) ?
                                    (float)(humpProtru / 2f * Math.Abs(Math.Cos(Math.PI * (i + humpGap / 4f) / (humpGap / 2f)))) :
                                    (float)(humpProtru / 2f * -Math.Abs(Math.Cos(Math.PI * (i + humpGap / 4f) / (humpGap / 2f)))) + humpProtru;
                case HumpStyle.PointyRound1Out: // Pointy / Round (out)
                    return (float)(humpProtru * -Math.Abs(Math.Sin(Math.PI * i / humpGap))) + humpProtru;
                case HumpStyle.PointyRound1In: // Pointy / Round (in)
                    return (float)(humpProtru * Math.Abs(Math.Cos(Math.PI * i / humpGap)));
                case HumpStyle.PointyRound2In: // Pointy & Round (in)
                    return ((int)((i + humpGap / 2f) / humpGap) % 2 != 0) ?
                                    (float)(humpProtru * Math.Abs(Math.Cos(Math.PI * (i + humpGap) / humpGap))) :
                                    (float)(humpProtru * -Math.Abs(Math.Cos(Math.PI * (i + humpGap / 2f) / humpGap))) + humpProtru;
                case HumpStyle.PointyRound2Out: // Pointy & Round (out)
                    return ((int)((i + humpGap / 2f) / humpGap) % 2 != 0) ?
                                    (float)(humpProtru * -Math.Abs(Math.Cos(Math.PI * (i + humpGap) / humpGap))) + humpProtru :
                                    (float)(humpProtru * Math.Abs(Math.Cos(Math.PI * (i + humpGap / 2f) / humpGap)));
            }
            return 0;
        }
    }
}
