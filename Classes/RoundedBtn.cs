using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace CustomControls.RJControls
{
    public class RoundedBtn : Button
    {
        //Fields
        private int borderSize = 0;
        private int borderRadius = 50;

        //Properties
        public int BorderSize
        {
            get => borderSize;
            set
            { borderSize = value; Invalidate(); }
        }
        public int BorderRadius
        {
            get => borderRadius;
            set
            { borderRadius = (value <= Height)?value:Height; Invalidate(); }
        }
        public Color BackgroundColor
        {
            get => BackColor; set { BackColor = value; }
        }
        public Color BorderColor
        {
            get => BackColor; set { BackColor = value; }
        }
        public Color TextColor
        {
            get => ForeColor; set { ForeColor = value; }
        }
        //Constructor
        public RoundedBtn()
        {
            Size = new Size(200, 100);
            FlatAppearance.BorderSize = 0;
            FlatStyle = FlatStyle.Flat;
            BackColor = Color.MediumSlateBlue;
            ForeColor = Color.White;
            BorderColor = Color.MediumSlateBlue;
            Resize += new EventHandler(Button_Resize);
        }

        //Methods
        private GraphicsPath GetFigurePath(RectangleF rectangle, float radius)
        {
            GraphicsPath path = new ();
            path.StartFigure();
            //float curveSize = radius * 2F;
            path.AddArc(rectangle.X, rectangle.Y, radius, radius, 180, 90);
            path.AddArc(rectangle.Width - radius, rectangle.Y, radius, radius, 270, 90);
            path.AddArc(rectangle.Width - radius, rectangle.Height - radius, radius, radius, 0, 90);
            path.AddArc(rectangle.X, rectangle.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF rectSurface = new RectangleF(0,0,Width,Height);
            RectangleF rectBorder = new RectangleF(1,1,Width-0.5f,Height-1);
            //int smoothSize = 2;
            if (borderRadius > 1)
            {
                using (GraphicsPath graphicsPathSurface = GetFigurePath(rectSurface, borderRadius))
                using (GraphicsPath graphicsPathBorder = GetFigurePath(rectBorder, borderRadius - 1f))
                using(Pen penSurface = new(Parent.BackColor,2))
                using(Pen penBorder = new(BorderColor,borderSize))
                {
                    penBorder.Alignment = PenAlignment.Inset;
                    Region = new(graphicsPathSurface);
                    pevent.Graphics.DrawPath(penBorder,graphicsPathSurface);
                    if (borderSize >= 1)
                        pevent.Graphics.DrawPath(penBorder,graphicsPathBorder);

                }

            }
            else
            {
                Region = new(rectSurface);
                if(borderSize >= 1)
                    using(Pen penBorder = new(BorderColor,borderSize))
                    {
                        penBorder.Alignment= PenAlignment.Inset;
                        pevent.Graphics.DrawRectangle(penBorder,0,0,Width-1,Height-1);
                    }
            }
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Parent.BackColorChanged += new EventHandler(Container_BackColorChanged);
        }

        private void Container_BackColorChanged(object sender, EventArgs e)
        {
            if(DesignMode)
            Invalidate();
        }
        private void Button_Resize(object sender, EventArgs e)
        {
            if (borderRadius > Height)
                borderRadius = Height;
        }
    }
}
