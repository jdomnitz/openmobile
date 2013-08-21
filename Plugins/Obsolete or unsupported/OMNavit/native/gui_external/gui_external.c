/**
 * Navit, a modular navigation system.
 * Copyright (C) 2005-2010 Navit Team
 *
 * _this program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * version 2 as published by the Free Software Foundation.
 *
 * _this program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with _this program; if not, write to the
 * Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA  02110-1301, USA.
 */

//##############################################################################################################
//#
//# File: gui_external.c
//# Description: external interface for OpenMobile GUI
//# Modifications by: Joseph Lukacovic (01/2012)
//#
//##############################################################################################################


#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <math.h>
#include <glib.h>
#include <time.h>
#include "config.h"
#ifdef HAVE_API_WIN32_BASE
#include <windows.h>
#endif
#include "item.h"
#include "file.h"
#include "navit.h"
#include "navit_nls.h"
#include "gui.h"
#include "coord.h"
#include "point.h"
#include "plugin.h"
#include "graphics.h"
#include "transform.h"
#include "color.h"
#include "map.h"
#include "layout.h"
#include "callback.h"
#include "vehicle.h"
#include "vehicleprofile.h"
#include "window.h"
#include "config_.h"
#include "keys.h"
#include "mapset.h"
#include "route.h"
#include "navit/search.h"
#include "track.h"
#include "country.h"
#include "config.h"
#include "event.h"
#include "navit_nls.h"
#include "navigation.h"
#include "gui_external.h"
#include "command.h"
#include "xmlconfig.h"
#include "util.h"
#include "bookmarks.h"
#include "debug.h"
#include "fib.h"
#include "types.h"


extern char *version;
#define PINVOKEENTRY _cdecl


struct gui_priv {
	struct navit *nav;
	struct attr self;
	struct window *win;
	struct graphics *gra;
	
	int pressed;
	int ignore_button;
	struct event_idle *idle;
	struct callback *motion_cb,*button_cb,*resize_cb,*keypress_cb,*window_closed_cb,*idle_cb, *motion_timeout_callback;
	struct event_timeout *motion_timeout_event;
	

	struct callback * vehicle_cb;
	struct callback_list *cbl;
	struct attr osd_configuration;

};

static struct gui_priv *gui_priv_static_pointer;

static void gui_external_motion(void *data, struct point *p)
{
	 dbg(1,"Func-> gui_external_motion\n");
	struct gui_priv *_this=data;
	navit_handle_motion(_this->nav, p);
}


static int
gui_external_get_attr(struct gui_priv *_this, enum attr_type type, struct attr *attr)
{
	 dbg(1,"Func-> gui_external_get_attr\n");
	
	return 1;
}

static int
gui_external_add_attr(struct gui_priv *_this, struct attr *attr)
{
	 dbg(1,"Func-> gui_external_add_attr\n");
	
	return 0;
}

static int
gui_external_set_attr(struct gui_priv *_this, struct attr *attr)
{
	dbg(1,"Func-> gui_external_set_attr\n");
	
	dbg(0,"%s\n",attr_to_name(attr->type));
	return 1;
	
}

static void gui_external_button(void *data, int pressed, int button, struct point *p)
{
	 dbg(1,"Func-> gui_external_button\n");
	struct gui_priv *_this=data;
	
	navit_handle_button(_this->nav, pressed, button, p, NULL);

}


static void gui_external_disable_suspend(struct gui_priv *_this)
{
	 dbg(1,"Func-> gui_external_disable_suspend\n");
	if (_this->win->disable_suspend)
		_this->win->disable_suspend(_this->win);
}

static void
gui_external_setup(struct gui_priv *_this)
{
	 dbg(1,"Func-> gui_external_setup\n");
}

static void gui_external_resize(void *data, int w, int h)
{
	 dbg(1,"Func-> gui_external_resize\n");
	struct gui_priv *_this=data;
	

	gui_external_setup(_this);

	navit_handle_resize(_this->nav, w, h);
}

static int gui_external_set_graphics(struct gui_priv *_this, struct graphics *gra)
{
	struct window *win;

	win=graphics_get_data(gra, "window");
        if (! win)
                return 1;
	navit_ignore_graphics_events(_this->nav, 1);
	_this->gra=gra;
	_this->win=win;
	navit_ignore_graphics_events(_this->nav, 1);
	//transform_get_size(trans, &_this->root.w, &_this->root.h);
	_this->resize_cb=callback_new_attr_1(callback_cast(gui_external_resize), attr_resize, _this);
	graphics_add_callback(gra, _this->resize_cb);
	_this->button_cb=callback_new_attr_1(callback_cast(gui_external_button), attr_button, _this);
	graphics_add_callback(gra, _this->button_cb);
	_this->motion_cb=callback_new_attr_1(callback_cast(gui_external_motion), attr_motion, _this);
	graphics_add_callback(gra, _this->motion_cb);
	
	/* Was resize callback already issued? */
	if (navit_get_ready(_this->nav) & 2)
		gui_external_setup(_this);
	return 0;
}

struct gui_methods gui_external_methods = {
	NULL,
	NULL,
        gui_external_set_graphics,
	NULL,
	NULL,
	NULL,
	gui_external_disable_suspend,
	gui_external_get_attr,
	gui_external_add_attr,
	gui_external_set_attr,
};

static struct gui_priv * gui_external_new(struct navit *nav, struct gui_methods *meth, struct attr **attrs, struct gui *gui)
{
	struct gui_priv *_this;
	
	*meth=gui_external_methods;
	_this=g_new0(struct gui_priv, 1);
	gui_priv_static_pointer = _this;
	_this->nav=nav;

	_this->self.type=attr_gui;
	_this->self.u.gui=gui;

	_this->cbl=callback_list_new();

	return _this;
}

void plugin_init(void)
{
	plugin_register_gui_type("external", gui_external_new);
}

static void
gui_external_destroy(struct gui_priv *_this)
{
	 dbg(1,"Func-> gui_external_destroy\n");
	g_free(_this);
}


static void
gui_external_search_list_set_default_country(struct search_list *_this)
{
	 dbg(1,"Func-> gui_external_search_list_set_default_country\n");
	struct attr search_attr, country_name, *country_attr;
	struct item *item;
	struct country_search *cs;
	struct search_list_result *res;

	country_attr=country_default();

	if (country_attr) {
		cs=country_search_new(country_attr, 0);
		item=country_search_get_item(cs);
		if (item && item_attr_get(item, attr_country_name, &country_name)) {
			search_attr.type=attr_country_all;
			dbg(0,"country %s\n", country_name.u.str);
			search_attr.u.str=country_name.u.str;
			search_list_search(_this, &search_attr, 0);
			while((res=search_list_get_result(_this)));
			/*if(_this->country_iso2) {
				g_free(_this->country_iso2);
				_this->country_iso2=NULL;
			}
			if (item_attr_get(item, attr_country_iso2, &country_iso2))
				_this->country_iso2=g_strdup(country_iso2.u.str);*/
		}
		country_search_destroy(cs);
	}/* else {
		dbg(0,"warning: no default country found\n");
		if (_this->country_iso2) {
		    dbg(0,"attempting to use country '%s'\n",_this->country_iso2);
		    search_attr.type=attr_country_iso2;
		    search_attr.u.str=_this->country_iso2;
            search_list_search(_this->sl, &search_attr, 0);
            while((res=search_list_get_result(_this->sl)));
		}
	}*/
}


static void
gui_external_get_direction(char *buffer, int angle, int mode)
{
	angle=angle%360;
	switch (mode) {
	case 0:
		sprintf(buffer,"%d",angle);
		break;
	case 1:
		if (angle < 69 || angle > 291)
			*buffer++='N';
		if (angle > 111 && angle < 249)
			*buffer++='S';
		if (angle > 22 && angle < 158)
			*buffer++='E';
		if (angle > 202 && angle < 338)
			*buffer++='W';
		*buffer++='\0';
		break;
	case 2:
		angle=(angle+15)/30;
		if (! angle)
			angle=12;
		sprintf(buffer,"%d H", angle);
		break;
	}
}


struct item_data {
	int dist;
	char *label;
	struct item item;
	struct coord c;
};

struct address_search_request 
{	
	char* country;
	char* town;
	char* postal;
	char* street;
	char* house_number;
};

struct address_search_result {
	int id;
	char* country;
	char* town;
	char* postal;
	char* street;
	char* house_number;
	struct address_search_result *next;
		
	struct coord_geo coord;
};

struct external_linked_list {
	void *first;
	void *last;
	int count;
};

struct poi_search_result {
	int type;
	float distance;
	char* direction;
	char* name;
	struct coord_geo coord;

	struct poi_search_result* next;
};

static void
gui_external_search_results_add(struct search_list_result *result, GList **result_array)
{
	struct address_search_result *new_result = g_new0(struct address_search_result, 1);
	*result_array = g_list_append(*result_array, new_result);

	if(result->c)
	{
		struct coord c;
		c.x = result->c->x;
		c.y = result->c->y;

		transform_to_geo(result->c->pro, &c, &new_result->coord);
	}

	new_result->id = result->id;
	new_result->country = result->country->name;
	if(result->town)
	{
		new_result->town = result->town->common.town_name;
		new_result->postal = result->town->common.postal;
	}
	if(result->street)
		new_result->street = result->street->name;

	if(result->house_number)
		new_result->house_number = result->house_number->house_number;
}

static void
gui_exernal_poi_results_add(struct item_data *poi_data, GList **result_array, struct coord *center, enum projection proj)
{
	struct poi_search_result *new_result = g_new0(struct poi_search_result, 1);
	*result_array = g_list_append(*result_array, new_result);
	
	char direction_buf[32];

	gui_external_get_direction(direction_buf, transform_get_angle_delta(center, &poi_data->c, 0), 1);

	new_result->name = poi_data->label;
	new_result->direction = g_strdup(direction_buf);
	transform_to_geo(proj, &poi_data->c, &new_result->coord);
	new_result->type = poi_data->item.type;
	new_result->distance = (float)poi_data->dist / 1000;
}

struct GList* PINVOKEENTRY gui_external_search_address(struct address_search_request *request, int max_results)
{
	struct search_list *sl;
	struct mapset *ms=navit_get_mapset(gui_priv_static_pointer->nav);
	
	struct search_list_result *result;
	//struct external_linked_list *results = g_new0(struct external_linked_list, 1);
	
	GList *results = NULL;
	sl=search_list_new(ms);
	
	int resultCt = 0;

	if(request->country)
	{
		struct attr country_attr;
		country_attr.type = attr_country_all;
		country_attr.u.str = request->country;
		search_list_search(sl, &country_attr, 1);
		while(search_list_get_result(sl));
	}
	else
	{
		gui_external_search_list_set_default_country(sl);
	}

	if(request->town)
	{
		struct attr town_attr;
		town_attr.type = attr_town_or_district_name;
		town_attr.u.str = request->town;
		search_list_search(sl, &town_attr, 1);
		
		
		while((result = search_list_get_result(sl))
			&& resultCt < max_results)
		{
			resultCt++;
			gui_external_search_results_add(result, &results);
		}
	}
	if(request->street)
	{
		struct attr street_attr;
		street_attr.type = attr_street_name;
		street_attr.u.str = request->street;
		search_list_search(sl, &street_attr, 1);
		
		while((result = search_list_get_result(sl))
			&& resultCt < max_results)
		{
			resultCt++;
			gui_external_search_results_add(result, &results);
		}
	}

	//if(request->house_number)
	//{
	//	struct attr house_attr;
	//	house_attr.type = attr_house_number;
	//	house_attr.u.str = request->house_number;
	//	search_list_search(sl, &house_attr, 1);
	//	
	//	while((result = search_list_get_result(sl))
	//		&& results->count < max_results)
	//	{
	//		gui_external_search_results_add(result, results);
	//	}
	//}
	
	dbg(1,"Func-> gui_external_search_town\n");
	
	search_list_destroy(sl);

	return results;
}

void PINVOKEENTRY gui_external_set_destination_from_coord(struct coord_geo coord, const char* description)
{	
	struct gui_priv* this_ = gui_priv_static_pointer;
	struct transformation *trans = navit_get_trans(this_->nav);

	struct pcoord pc;
	struct coord c;

	pc.pro = transform_get_projection(trans);//->trans);

	transform_from_geo(pc.pro, &coord, &c);

	pc.x = c.x;
	pc.y = c.y;

	navit_set_destination(gui_priv_static_pointer->nav, &pc, description, 0);
}

///////////////// POIS

struct selector {
	char *icon;
	char *name;
	enum item_type *types;
};
struct selector gui_external_selectors[]={
	{"bank","Bank",(enum item_type []){
		type_poi_bank,type_poi_bank,
		type_poi_atm,type_poi_atm,
		type_none}},
	{"fuel","Fuel",(enum item_type []){type_poi_fuel,type_poi_fuel,type_none}},
	{"hotel","Hotel",(enum item_type []) {
		type_poi_hotel,type_poi_camp_rv,
		type_poi_camping,type_poi_camping,
		type_poi_resort,type_poi_resort,
		type_poi_motel,type_poi_hostel,
		type_none}},
	{"restaurant","Restaurant",(enum item_type []) {
		type_poi_bar,type_poi_picnic,
		type_poi_burgerking,type_poi_fastfood,
		type_poi_restaurant,type_poi_restaurant,
		type_poi_cafe,type_poi_cafe,
		type_poi_pub,type_poi_pub,
		type_none}},
	{"shopping","Shopping",(enum item_type []) {
		type_poi_mall,type_poi_mall,
		type_poi_shop_grocery,type_poi_shop_grocery,
		type_poi_shopping,type_poi_shopping,
		type_poi_shop_butcher,type_poi_shop_baker,
		type_poi_shop_fruit,type_poi_shop_fruit,
		type_poi_shop_beverages,type_poi_shop_beverages,
		type_none}},
	{"hospital","Service",(enum item_type []) {
		type_poi_marina,type_poi_marina,
		type_poi_hospital,type_poi_hospital,
		type_poi_public_utilities,type_poi_public_utilities,
		type_poi_police,type_poi_autoservice,
		type_poi_information,type_poi_information,
		type_poi_pharmacy,type_poi_pharmacy,
		type_poi_personal_service,type_poi_repair_service,
		type_poi_restroom,type_poi_restroom,
		type_none}},
	{"parking","Parking",(enum item_type []){type_poi_car_parking,type_poi_car_parking,type_none}},
	{"peak","Land Features",(enum item_type []){
		type_poi_land_feature,type_poi_rock,
		type_poi_dam,type_poi_dam,
		type_poi_peak,type_poi_peak,
		type_none}},
	{"unknown","Other",(enum item_type []){
		type_point_unspecified,type_poi_land_feature-1,
		type_poi_rock+1,type_poi_fuel-1,
		type_poi_marina+1,type_poi_shopping-1,
		type_poi_shopping+1,type_poi_car_parking-1,
		type_poi_car_parking+1,type_poi_bar-1,
		type_poi_bank+1,type_poi_dam-1,
		type_poi_dam+1,type_poi_information-1,
		type_poi_information+1,type_poi_mall-1,
		type_poi_mall+1,type_poi_personal_service-1,
		type_poi_pharmacy+1,type_poi_repair_service-1,
		type_poi_repair_service+1,type_poi_restaurant-1,
		type_poi_restaurant+1,type_poi_restroom-1,
		type_poi_restroom+1,type_poi_shop_grocery-1,
		type_poi_shop_grocery+1,type_poi_peak-1,
		type_poi_peak+1, type_poi_motel-1,
		type_poi_hostel+1,type_poi_shop_butcher-1,
		type_poi_shop_baker+1,type_poi_shop_fruit-1,
		type_poi_shop_fruit+1,type_poi_shop_beverages-1,
		type_poi_shop_beverages+1,type_poi_pub-1,
		type_poi_atm+1,type_line-1,
		type_none}},
/*	{"unknown","Unknown",(enum item_type []){
		type_point_unkn,type_point_unkn,
		type_none}},*/
};


/*
 *  Get a utf-8 string, return the same prepared for case insensetive search. Result shoud be g_free()d after use.
 */

static char *
removecase(char *s) 
{
	char *r;
	r=g_utf8_casefold(s,-1);
	return r;
}

/**
 * POI search/filtering parameters.
 *
 */

struct poi_param {

		/**
 		 * =1 if selnb is defined, 0 otherwize.
		 */
		unsigned char sel;
		/**
 		 * Index to struct selector selectors[], shows what type of POIs is defined.
		 */		
		unsigned char selnb;
		/**
		* Elements per Page
		*/
		int pageCount;
		/**
 		 * Page number to display.
		 */		
		unsigned char pagenb;
		/**
 		 * Radius (number of 10-kilometer intervals) to search for POIs.
		 */		
		unsigned char dist;
		/**
 		 * Should filter phrase be compared to postal address of the POI.
 		 * =1 - address filter, =0 - name filter
		 */		
		unsigned char isAddressFilter;
		/**
 		 * Filter string, casefold()ed and divided into substrings at the spaces, which are replaced by ASCII 0*.
		 */		
		char *filterstr; 
		/**
 		 * list of pointers to individual substrings of filterstr.
		 */		
		GList *filter;
};


/**
 * @brief Set POIs filter data in poi_param structure.
 * @param param poi_param structure with unset filter data.
 */
static void
gui_external_poi_param_set_filter(struct poi_param *param) 
{
	char *s1, *s2;
	if(!param->filterstr
		|| param->filterstr == "")
		return;

	param->filterstr=removecase(param->filterstr);
	s1=param->filterstr;
	do {
		s2=g_utf8_strchr(s1,-1,' ');
		if(s2)
			*s2++=0;
		param->filter=g_list_append(param->filter,s1);
		if(s2) {
			while(*s2==' ')
				s2++;
		}
		s1=s2;
	} while(s2 && *s2);
}


char *
gui_external_compose_item_address_string(struct item *item)
{
	char *s=g_strdup("");
	struct attr attr;
	if(item_attr_get(item, attr_house_number, &attr)) 
		s=g_strjoin(" ",s,attr.u.str,NULL);
	if(item_attr_get(item, attr_street_name, &attr)) 
		s=g_strjoin(" ",s,attr.u.str,NULL);
	if(item_attr_get(item, attr_street_name_systematic, &attr)) 
		s=g_strjoin(" ",s,attr.u.str,NULL);
	if(item_attr_get(item, attr_district_name, &attr)) 
		s=g_strjoin(" ",s,attr.u.str,NULL);
	if(item_attr_get(item, attr_town_name, &attr)) 
		s=g_strjoin(" ",s,attr.u.str,NULL);
	if(item_attr_get(item, attr_county_name, &attr)) 
		s=g_strjoin(" ",s,attr.u.str,NULL);
	if(item_attr_get(item, attr_country_name, &attr)) 
		s=g_strjoin(" ",s,attr.u.str,NULL);
	
	if(item_attr_get(item, attr_address, &attr)) 
		s=g_strjoin(" ",s,"|",attr.u.str,NULL);
	return s;
}

static int
gui_external_cmd_pois_item_selected(struct poi_param *param, struct item *item)
{
	enum item_type *types;
	struct selector *sel = param->sel? &gui_external_selectors[param->selnb]: NULL;
	enum item_type type=item->type;
	struct attr attr;
	int match=0;
	if (type >= type_line && param->filter==NULL)
		return 0;
	if (! sel || !sel->types) {
		match=1;
	} else {
		types=sel->types;
		while (*types != type_none) {
			if (item->type >= types[0] && item->type <= types[1]) {
				return 1;
			}

			types+=2;
		}
	}
	if (param->filter) {
		char *long_name, *s;
		GList *f;
		if (param->isAddressFilter) {
			s=gui_external_compose_item_address_string(item);
		} else if (item_attr_get(item, attr_label, &attr)) {
			s=g_strdup_printf("%s %s", item_to_name(item->type), attr.u.str);
		} else {
			s=g_strdup(item_to_name(item->type));
		}
		long_name=removecase(s);
		g_free(s);
                item_attr_rewind(item);
                
		for(s=long_name,f=param->filter;f && s;f=g_list_next(f)) {
			s=strstr(s,f->data);
			if(!s) 
				break;
			s=g_utf8_strchr(s,-1,' ');
		}
		if(f)
			match=0;
		g_free(long_name);
	}
	return match;
}

struct GList* PINVOKEENTRY gui_external_get_points_of_interest(void *params)
{
	struct gui_priv *this_ = gui_priv_static_pointer;

	struct map_selection *sel,*selm;
	struct coord c;//,center;
	struct mapset_handle *h;
	struct map *m;
	struct map_rect *mr;
	struct item *item;
	struct transformation *trans = navit_get_trans(this_->nav);
	enum projection pro= transform_get_projection(trans);
	struct coord *center = transform_get_center(trans);
	struct poi_param *param;
	int param_free=0;
	int idist,dist;
	struct selector *isel;
	int pagenb;
	int prevdist;
	// Starting value and increment of count of items to be extracted
	int pagesize = 50; 
	int maxitem, it = 0, i;
	struct item_data *items;
	struct fibheap* fh = fh_makekeyheap();
	int cnt = 0;
	struct pcoord pc;

	//center->x = -8118361;
	//center->y = 5127105;

	pc.pro = transform_get_projection(trans);
	pc.x = center->x;
	pc.y = center->y;

	if(params) {
	  param = (struct poi_param*)params;
	  pagesize = param->pageCount;
	  gui_external_poi_param_set_filter(param);
	} else {
	  param = g_new0(struct poi_param,1);
	  param_free=1;
	}
	// my params
	  //param->dist = 3;


	dist=10000*(param->dist+1);
	isel = param->sel? &gui_external_selectors[param->selnb]: NULL;
	pagenb = param->pagenb;
	prevdist=param->dist*10000;
	maxitem = pagesize*(pagenb+1);
	items = g_new0( struct item_data, maxitem);
	
	dbg(0, "Params: sel = %i, selnb = %i, pagenb = %i, dist = %i, filterstr = %s, isAddressFilter= %d\n",
		param->sel, param->selnb, param->pagenb, param->dist, param->filterstr, param->isAddressFilter);

	sel=map_selection_rect_new(&pc ,dist*transform_scale(abs(center->y)+dist*1.5),18);
	
	h=mapset_open(navit_get_mapset(this_->nav));
        while ((m=mapset_next(h, 1))) {
		selm=map_selection_dup_pro(sel, pro, map_projection(m));
		mr=map_rect_new(m, selm);
		dbg(2,"mr=%p\n", mr);
		if (mr) {
			while ((item=map_rect_get_item(mr))) {
				if (gui_external_cmd_pois_item_selected(param, item) &&
				    item_coord_get_pro(item, &c, 1, pro) &&
				    coord_rect_contains(&sel->u.c_rect, &c)  &&
				    (idist=transform_distance(pro, center, &c)) < dist) {
					
					struct item_data *data;
					struct attr attr;
					char *label;
					
					if (item->type==type_house_number) {
						label=gui_external_compose_item_address_string(item);
					} else if (item_attr_get(item, attr_label, &attr)) {
						label=g_strdup(attr.u.str);
						// Buildings which label is equal to addr:housenumber value
						// are duplicated by item_house_number. Don't include such 
						// buildings into the list. this_ is true for OSM maps created with 
						// maptool patched with #859 latest patch.
						// FIXME: For non-OSM maps, we probably would better don't skip these items.
						if(item->type==type_poly_building && item_attr_get(item, attr_house_number, &attr) ) {
							if(strcmp(label,attr.u.str)==0) {
								g_free(label);
								continue;
							}
						}

					} else {
						label=g_strdup("");
					}
					
					if(it>=maxitem) {
						data = fh_extractmin(fh);
						g_free(data->label);
						data->label=NULL;
					} else {
						data = &items[it++];
					}
					data->label=label;
					data->item = *item;
					data->c = c;
					data->dist = idist;
					// Key expression is a workaround to fight
					// probable heap collisions when two objects
					// are at the same distance. But it destroys
					// right order of POIs 2048 km away from cener
					// and if table grows more than 1024 rows.
					fh_insertkey(fh, -((idist<<10) + cnt++), data);
					if (it == maxitem)
						dist = (-fh_minkey(fh))>>10;
				}
			}
			map_rect_destroy(mr);
		}
		map_selection_destroy(selm);
	}
	map_selection_destroy(sel);
	mapset_close(h);

	GList* list = NULL;
	// Move items from heap to the table
	for(i=0;;i++) 
	{
		int key = fh_minkey(fh);
		struct item_data *data = fh_extractmin(fh);
		if (data == NULL)
		{
			dbg(2, "Empty heap: maxitem = %i, it = %i, dist = %i\n", maxitem, it, dist);
			break;
		}
		dbg(2, "dist1: %i, dist2: %i\n", data->dist, (-key)>>10);
		if(i==(it-pagesize*pagenb) && data->dist>prevdist)
			prevdist=data->dist;


		gui_exernal_poi_results_add(data, &list, center, pc.pro);

	}
	
	fh_deleteheap(fh);
	free(items);
	
	dbg(1,"Returning gracefully\n");
	return list;
}

void PINVOKEENTRY
gui_external_clear_destination()
{
	struct gui_priv* this_ = gui_priv_static_pointer;

	navit_set_destination(this_->nav, NULL, NULL, 0);
}

struct coord_geo* PINVOKEENTRY
gui_external_get_current_position()
{
	struct gui_priv *this_ = gui_priv_static_pointer;
	struct tracking *track = navit_get_tracking(this_->nav);
	struct transformation *trans = navit_get_trans(this_->nav);
	
	
	struct coord_geo *cg = g_new0(struct coord_geo, 1);
	struct coord *c;

	enum projection pro = transform_get_projection(trans);

	c = tracking_get_pos(track);
	
	transform_to_geo(pro, c, cg);

	return cg;
}

struct map_item 
{
	char* name;
	int type;
	struct coord_geo coord;
};

struct map_item* PINVOKEENTRY
gui_external_get_map_item_at_screen_location(struct point p)
{	
	struct gui_priv* this_ = gui_priv_static_pointer;
	struct transformation *trans;
	struct coord c;
	
	trans=navit_get_trans(this_->nav);

	enum projection pro = transform_get_projection(trans);
	struct map_item* item = g_new0(struct map_item, 1);

	
	transform_reverse(trans, &p, &c);
	dbg(1,"x=0x%x y=0x%x\n", c.x, c.y);

	transform_to_geo(pro, &c, &item->coord);
	
	// TODO: Name and Attr_type

	/*this->clickp.pro=transform_get_projection(trans);
	this->clickp.x=c.x;
	this->clickp.y=c.y;*/

	return item;
}

void PINVOKEENTRY
gui_external_destroy_map_item(struct map_item *item)
{
	if(item 
		&& item->name)
		free(item->name);
	g_free(item);
}

void
gui_external_destroy_address_search_result(struct address_search_result *destroy_me, gpointer user_data)
{
	// These strings probably shouldn't be free'd..?
}

void
gui_external_destroy_poi_search_result(struct poi_search_result *destroy_me, gpointer user_data)
{
	g_free(destroy_me->name);
	g_free(destroy_me->direction);
	g_free(destroy_me);
}

void PINVOKEENTRY 
gui_external_destroy_address_search_results(GList *destroy_me)
{
	g_list_foreach(destroy_me, (GFunc)gui_external_destroy_address_search_result, NULL);
	g_list_free(destroy_me);
}

void PINVOKEENTRY 
gui_external_destroy_poi_search_results(GList *destroy_me)
{
	g_list_foreach(destroy_me, (GFunc)gui_external_destroy_poi_search_result, NULL);
	g_list_free(destroy_me);
}


void PINVOKEENTRY gui_external_free_memory(void* pointer)
{
	g_free(pointer);
}
