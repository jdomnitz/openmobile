struct gui_external_methods {
	void (*add_callback)(struct gui_priv *priv, struct callback *cb);
	void (*remove_callback)(struct gui_priv *priv, struct callback *cb);
	void (*menu_render)(struct gui_priv *this);
	struct graphics_image * (*image_new_xs)(struct gui_priv *this, const char *name);
	struct graphics_image * (*image_new_l)(struct gui_priv *this, const char *name);
};

