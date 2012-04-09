
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.UIManager UIManager;
	private global::Gtk.Action FileAction;
	private global::Gtk.Action OpenAction;
	private global::Gtk.Action ExitAction;
	private global::Gtk.Action HelpAction;
	private global::Gtk.Action AboutAction;
	private global::Gtk.VBox vbox1;
	private global::Gtk.MenuBar MainMenuBar;
	private global::Gtk.HBox hbox1;
	private global::Gtk.Alignment alignment1;
	private global::Gtk.HPaned hpaned1;
	private global::Gtk.ScrolledWindow GtkScrolledWindow;
	private global::Gtk.TreeView assemblyView;
	private global::Gtk.VPaned vpaned1;
	private global::Gtk.ComboBox combobox6;
	private global::Gtk.ScrolledWindow GtkScrolledWindow1;
	private global::Gtk.TextView disassemblyText;
	
	protected virtual void Build ()
	{
		global::Stetic.Gui.Initialize (this);
		// Widget MainWindow
		this.UIManager = new global::Gtk.UIManager ();
		global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
		this.FileAction = new global::Gtk.Action ("FileAction", global::Mono.Unix.Catalog.GetString ("File"), null, null);
		this.FileAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("File");
		w1.Add (this.FileAction, null);
		this.OpenAction = new global::Gtk.Action ("OpenAction", global::Mono.Unix.Catalog.GetString ("Open"), null, null);
		this.OpenAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Open");
		w1.Add (this.OpenAction, null);
		this.ExitAction = new global::Gtk.Action ("ExitAction", global::Mono.Unix.Catalog.GetString ("Exit"), null, null);
		this.ExitAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Exit");
		w1.Add (this.ExitAction, null);
		this.HelpAction = new global::Gtk.Action ("HelpAction", global::Mono.Unix.Catalog.GetString ("Help"), null, null);
		this.HelpAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Help");
		w1.Add (this.HelpAction, null);
		this.AboutAction = new global::Gtk.Action ("AboutAction", global::Mono.Unix.Catalog.GetString ("About..."), null, null);
		this.AboutAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("About...");
		w1.Add (this.AboutAction, null);
		this.UIManager.InsertActionGroup (w1, 0);
		this.AddAccelGroup (this.UIManager.AccelGroup);
		this.Name = "MainWindow";
		this.Title = global::Mono.Unix.Catalog.GetString ("SolidReflector");
		this.WindowPosition = ((global::Gtk.WindowPosition)(3));
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.vbox1 = new global::Gtk.VBox ();
		this.vbox1.Name = "vbox1";
		this.vbox1.Spacing = 6;
		// Container child vbox1.Gtk.Box+BoxChild
		this.UIManager.AddUiFromString ("<ui><menubar name='MainMenuBar'><menu name='FileAction' action='FileAction'><menuitem name='OpenAction' action='OpenAction'/><menuitem name='ExitAction' action='ExitAction'/></menu><menu name='HelpAction' action='HelpAction'><menuitem name='AboutAction' action='AboutAction'/></menu></menubar></ui>");
		this.MainMenuBar = ((global::Gtk.MenuBar)(this.UIManager.GetWidget ("/MainMenuBar")));
		this.MainMenuBar.Name = "MainMenuBar";
		this.vbox1.Add (this.MainMenuBar);
		global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.MainMenuBar]));
		w2.Position = 0;
		w2.Expand = false;
		w2.Fill = false;
		// Container child vbox1.Gtk.Box+BoxChild
		this.hbox1 = new global::Gtk.HBox ();
		this.hbox1.Name = "hbox1";
		this.hbox1.Spacing = 6;
		// Container child hbox1.Gtk.Box+BoxChild
		this.alignment1 = new global::Gtk.Alignment (0.5F, 0.5F, 1F, 1F);
		this.alignment1.Name = "alignment1";
		// Container child alignment1.Gtk.Container+ContainerChild
		this.hpaned1 = new global::Gtk.HPaned ();
		this.hpaned1.CanFocus = true;
		this.hpaned1.Name = "hpaned1";
		this.hpaned1.Position = 308;
		// Container child hpaned1.Gtk.Paned+PanedChild
		this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow.Name = "GtkScrolledWindow";
		this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
		this.assemblyView = new global::Gtk.TreeView ();
		this.assemblyView.CanFocus = true;
		this.assemblyView.Name = "assemblyView";
		this.GtkScrolledWindow.Add (this.assemblyView);
		this.hpaned1.Add (this.GtkScrolledWindow);
		global::Gtk.Paned.PanedChild w4 = ((global::Gtk.Paned.PanedChild)(this.hpaned1 [this.GtkScrolledWindow]));
		w4.Resize = false;
		// Container child hpaned1.Gtk.Paned+PanedChild
		this.vpaned1 = new global::Gtk.VPaned ();
		this.vpaned1.CanFocus = true;
		this.vpaned1.Name = "vpaned1";
		this.vpaned1.Position = 31;
		// Container child vpaned1.Gtk.Paned+PanedChild
		this.combobox6 = global::Gtk.ComboBox.NewText ();
		this.combobox6.AppendText (global::Mono.Unix.Catalog.GetString ("IL"));
		this.combobox6.AppendText (global::Mono.Unix.Catalog.GetString ("CFG"));
		this.combobox6.Name = "combobox6";
		this.combobox6.Active = 1;
		this.vpaned1.Add (this.combobox6);
		global::Gtk.Paned.PanedChild w5 = ((global::Gtk.Paned.PanedChild)(this.vpaned1 [this.combobox6]));
		w5.Resize = false;
		// Container child vpaned1.Gtk.Paned+PanedChild
		this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
		this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
		this.disassemblyText = new global::Gtk.TextView ();
		this.disassemblyText.CanFocus = true;
		this.disassemblyText.Name = "disassemblyText";
		this.disassemblyText.Editable = false;
		this.disassemblyText.CursorVisible = false;
		this.GtkScrolledWindow1.Add (this.disassemblyText);
		this.vpaned1.Add (this.GtkScrolledWindow1);
		this.hpaned1.Add (this.vpaned1);
		this.alignment1.Add (this.hpaned1);
		this.hbox1.Add (this.alignment1);
		global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.alignment1]));
		w10.Position = 0;
		this.vbox1.Add (this.hbox1);
		global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.hbox1]));
		w11.Position = 1;
		this.Add (this.vbox1);
		if ((this.Child != null)) {
			this.Child.ShowAll ();
		}
		this.DefaultWidth = 780;
		this.DefaultHeight = 723;
		this.Show ();
		this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
		this.OpenAction.Activated += new global::System.EventHandler (this.OnOpenActionActivated);
		this.ExitAction.Activated += new global::System.EventHandler (this.OnExitActionActivated);
		this.assemblyView.RowActivated += new global::Gtk.RowActivatedHandler (this.OnAssemblyViewRowActivated);
		this.combobox6.Changed += new global::System.EventHandler (this.OnCombobox6Changed);
	}
}
