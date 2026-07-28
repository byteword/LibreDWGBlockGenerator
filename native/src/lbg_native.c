#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <dwg.h>
#include <dwg_api.h>

#if defined(_WIN32)
#  if defined(LBG_NATIVE_EXPORTS)
#    define LBG_API __declspec(dllexport)
#  else
#    define LBG_API __declspec(dllimport)
#  endif
#else
#  define LBG_API __attribute__((visibility("default")))
#endif

enum
{
  LBG_SUCCESS = 0,
  LBG_INVALID_ARGUMENT = 2,
  LBG_GRAPH_FAILURE = 4,
  LBG_ENCODING_FAILURE = 5,
  LBG_VERIFICATION_FAILURE = 6
};

LBG_API int
lbg_generate_r2000_polyline_block (const char *output_path,
                                    const char *block_name,
                                    const double *origin_xyz,
                                    const double *xy,
                                    const size_t point_count,
                                    const int closed)
{
  Dwg_Data *dwg;
  Dwg_Data verify;
  Dwg_Object *model_space;
  Dwg_Object_BLOCK_HEADER *model_header;
  Dwg_Object_BLOCK_HEADER *block_header;
  Dwg_Object_BLOCK_CONTROL *block_control;
  Dwg_Entity_LWPOLYLINE *polyline;
  dwg_point_2d *points;
  dwg_point_3d insertion = { 0.0, 0.0, 0.0 };
  int error;
  int found_block = 0;
  size_t index;

  if (!output_path || !block_name || !origin_xyz || !xy || point_count < 2
      || block_name[0] == '\0')
    return LBG_INVALID_ARGUMENT;

  points = (dwg_point_2d *)calloc (point_count, sizeof (dwg_point_2d));
  if (!points)
    return LBG_GRAPH_FAILURE;
  for (index = 0; index < point_count; index++)
    {
      points[index].x = xy[index * 2];
      points[index].y = xy[index * 2 + 1];
    }

  dwg = dwg_new_Document (R_2000, 0, 0);
  if (!dwg)
    {
      free (points);
      return LBG_GRAPH_FAILURE;
    }

  model_space = dwg_model_space_object (dwg);
  model_header = model_space && model_space->tio.object
                   ? model_space->tio.object->tio.BLOCK_HEADER
                   : NULL;
  block_header = dwg_add_BLOCK_HEADER (dwg, block_name);
  if (!model_header || !block_header || !dwg_add_BLOCK (block_header, block_name))
    {
      free (points);
      dwg_free (dwg);
      return LBG_GRAPH_FAILURE;
    }
  block_header->base_pt.x = origin_xyz[0];
  block_header->base_pt.y = origin_xyz[1];
  block_header->base_pt.z = origin_xyz[2];

  polyline = dwg_add_LWPOLYLINE (block_header, (int)point_count, points);
  free (points);
  if (!polyline)
    {
      dwg_free (dwg);
      return LBG_GRAPH_FAILURE;
    }
  if (closed)
    polyline->flag |= 512;

  if (!dwg_add_ENDBLK (block_header)
      || !dwg_add_INSERT (model_header, &insertion, block_name,
                          1.0, 1.0, 1.0, 0.0))
    {
      dwg_free (dwg);
      return LBG_GRAPH_FAILURE;
    }

  error = dwg_write_file (output_path, dwg);
  dwg_free (dwg);
  if (error >= DWG_ERR_CRITICAL)
    {
      remove (output_path);
      return LBG_ENCODING_FAILURE;
    }

  memset (&verify, 0, sizeof (verify));
  error = dwg_read_file (output_path, &verify);
  if (error >= DWG_ERR_CRITICAL)
    {
      remove (output_path);
      return LBG_VERIFICATION_FAILURE;
    }
  block_control = dwg_block_control (&verify);
  if (block_control)
    {
      for (index = 0; index < block_control->num_entries; index++)
        {
          Dwg_Object_Ref *entry = block_control->entries[index];
          Dwg_Object_BLOCK_HEADER *header;
          char *name;
          if (!entry || !entry->obj || !entry->obj->tio.object)
            continue;
          header = entry->obj->tio.object->tio.BLOCK_HEADER;
          if (!header)
            continue;
          name = dwg_obj_block_header_get_name (header, &error);
          if (!error && name && strcmp (name, block_name) == 0)
            {
              found_block = 1;
              break;
            }
        }
    }
  dwg_free (&verify);
  if (!found_block)
    {
      remove (output_path);
      return LBG_VERIFICATION_FAILURE;
    }
  return LBG_SUCCESS;
}
