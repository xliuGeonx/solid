/*
 * $Id$
 * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
 * For further details see the nearest License.txt
 */

using Cairo;

using System;
using System.Collections.Generic;
using System.Text;

using SolidV;
using SolidV.MVC;

using DataMorphose.Model;

namespace DataMorphose.Plugins.Visualizer
{
  public class SchemaVisualizer
  {
    private Gtk.DrawingArea canvas = null;

    private ShapesModel scene = new ShapesModel();
    public ShapesModel Scene {
      get { return scene; }
      set { scene = value; }
    }

    private SelectionModel selection = new SelectionModel();
    public SelectionModel Selection {
      get { return selection; }
    }

    private SolidV.MVC.Model model = new SolidV.MVC.Model();
    public SolidV.MVC.Model Model {
      get { return model; }
    }

    private InteractionStateModel interaction = new InteractionStateModel();
    private View<Context, SolidV.MVC.Model> view = new View<Context, SolidV.MVC.Model>();
    private CompositeController<Gdk.Event, Context, SolidV.MVC.Model> controller = 
                                    new CompositeController<Gdk.Event, Context, SolidV.MVC.Model>();


    public SchemaVisualizer(Gtk.DrawingArea canvas) {
      this.canvas = canvas;

      canvas.AddEvents((int) Gdk.EventMask.ButtonPressMask);
      canvas.AddEvents((int) Gdk.EventMask.ButtonReleaseMask);
      canvas.AddEvents((int) Gdk.EventMask.PointerMotionMask);

      canvas.ButtonPressEvent += HandleDrawingArea1ButtonPressEvent;
      canvas.ButtonReleaseEvent += HandleDrawingArea1ButtonReleaseEvent;
      canvas.KeyPressEvent += HandleKeyPressEvent;
      canvas.KeyReleaseEvent += HandleKeyReleaseEvent;

      canvas.ExposeEvent += HandleDrawingArea1ExposeEvent;
      canvas.ConfigureEvent += HandleConfigureEvent;
      canvas.MotionNotifyEvent += HandleDrawingArea1MotionNotifyEvent;

      model.RegisterSubModel<ShapesModel>(scene);                                                                     
      model.RegisterSubModel<SelectionModel>(selection);                                                                          
      model.RegisterSubModel<InteractionStateModel>(interaction);                                                                 
      
      model.ModelChanged += HandleModelChanged;

      view.Viewers.Add(typeof(SolidV.MVC.Model), new ModelViewer<Context>());
      view.Viewers.Add(typeof(ShapesModel), new ShapeModelViewer());
      view.Viewers.Add(typeof(RectangleShape), new RectangleShapeViewer());
      view.Viewers.Add(typeof(EllipseShape), new EllipseShapeViewer());
      view.Viewers.Add(typeof(ArrowShape), new ArrowShapeViewer());
      view.Viewers.Add(typeof(TextBlockShape), new TextBlockShapeViewer());
      view.Viewers.Add(typeof(SelectionModel), new SelectionModelViewer());
      view.Viewers.Add(typeof(Glue), new GlueViewer());
      view.Viewers.Add(typeof(InteractionStateModel), new InteractionStateModelViewer());

      controller.SubControllers.Add(new ShapeSelectionController(model, view));
      ChainController<Gdk.Event, Context, SolidV.MVC.Model> cc = 
                                        new ChainController<Gdk.Event, Context, SolidV.MVC.Model>();
      cc.SubControllers.Add(new ConnectorDragController(model, view));
      cc.SubControllers.Add(new ShapeDragController(model, view));
      controller.SubControllers.Add(cc);
    }

    void HandleConfigureEvent (object o, Gtk.ConfigureEventArgs args) {
      controller.HandleEvent(args.Event);
    }

    /// <summary>
    /// Invalidate the canvas.
    /// </summary>
    /// <param name='model'>
    /// Model.
    /// </param>
    public void HandleModelChanged(object model) {
      canvas.QueueDraw();
    }

    public void HandleDrawingArea1ButtonPressEvent(object o, Gtk.ButtonPressEventArgs args) {
      canvas.GrabFocus();
      controller.HandleEvent(args.Event);
    }
    
    public void HandleDrawingArea1ButtonReleaseEvent(object o, Gtk.ButtonReleaseEventArgs args) {
      controller.HandleEvent(args.Event);
    }
    
    protected void HandleKeyPressEvent (object o, Gtk.KeyPressEventArgs args)
    {
      controller.HandleEvent(args.Event);
    }
    
    protected void HandleKeyReleaseEvent (object o, Gtk.KeyReleaseEventArgs args)
    {
      controller.HandleEvent(args.Event);
    }

    public void HandleDrawingArea1MotionNotifyEvent(object o, Gtk.MotionNotifyEventArgs args) {
      controller.HandleEvent(args.Event);
    }

    public void HandleDrawingArea1ExposeEvent(object o, Gtk.ExposeEventArgs args) {
      using (Cairo.Context context = Gdk.CairoHelper.Create(((Gtk.DrawingArea)o).GdkWindow)) {
        view.Draw(context, model);
      }
    }

    public void DrawSchema(Model.Table t) {
      Dictionary<string, TextBlockShape> basicBlocks = new Dictionary<string, TextBlockShape>();
      
      int x = 100, y = 100;
      TextBlockShape textBlock = new TextBlockShape(new Cairo.Rectangle(x, y, 40, 40),
                                                    /*autoSize*/true);
      
      textBlock.Style.Border = new SolidPattern(new Color(0, 0, 0));
      textBlock.Title = t.Name;
//      textBlock.FontSize = 12;
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < t.Columns.Count; i++) 
        sb.AppendLine(t.Columns[i].Meta.Name);
      
      textBlock.BlockText = sb.ToString();
      
      basicBlocks.Add(t.Name, textBlock);
      scene.Shapes.Add(textBlock);
    }

    public void DrawFilter() {
      List<ArrowShape> arrows = new List<ArrowShape>();

      int x = 100, y = 300;
      EllipseShape filter = new EllipseShape(new Cairo.Rectangle(x, y, 100, 50));
      filter.Style.Border = new SolidPattern(new Color(0, 0, 0));

      ConnectorGluePoint filterGlue = new ConnectorGluePoint(
      new PointD(filter.Location.X + (filter.Width/2), filter.Location.Y));
      filter.Items.Add(filterGlue);
      filterGlue.Parent = filter;
      
      for (int i = 0; i < scene.Shapes.Count; i++) {
        var block = scene.Shapes[i];
        ConnectorGluePoint blockGlue = new ConnectorGluePoint(new PointD(block.Location.X 
                                             + block.Width / 2, block.Location.Y + block.Height));
        blockGlue.Parent = block;
        // Add the gluePoints to the shapes
        block.Items.Add(blockGlue);

        // Link the filter and the textBlock with an arrow 
        ArrowShape arrow = new ArrowShape(block, blockGlue, filter, filterGlue);
        arrows.Add(arrow); 
      }
      
      model.BeginUpdate();
      // Add all shapes to the scene
      scene.Shapes.Add(filter);
      foreach(ArrowShape a in arrows)
        scene.Shapes.Add(a);

      model.EndUpdate();
    }
  }
}

