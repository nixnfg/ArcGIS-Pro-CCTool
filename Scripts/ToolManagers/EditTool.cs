using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping;
using CCTool.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core;
using ArcGIS.Core.Data.DDL;
using FieldDescription = ArcGIS.Core.Data.DDL.FieldDescription;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Core.CIM;
using Aspose.Cells.Drawing;

namespace CCTool.Scripts.ToolManagers
{
    public class EditTool
    {
        // 合并要素【按OID】
        public static void MergeFeatures(FeatureLayer featureLayer, List<long> objectIDs)
        {
            var mergeFeatures = new EditOperation();

            // 创建【inspector】实例
            var inspector = new Inspector();
            // 加载
            inspector.Load(featureLayer, objectIDs[0]);
            // 执行
            mergeFeatures.Merge(featureLayer, objectIDs, inspector);
            mergeFeatures.Execute();
            // 保存
            Project.Current.SaveEditsAsync();
        }

        // 合并要素【按OID】
        public static void MergeFeatures(FeatureLayer featureLayer, List<List<long>> objectIDs)
        {
            foreach (var item in objectIDs)
            {
                var mergeFeatures = new EditOperation();
                // 创建【inspector】实例
                var inspector = new Inspector();
                // 加载
                inspector.Load(featureLayer, objectIDs[0]);
                // 执行
                mergeFeatures.Merge(featureLayer, item, inspector);

                mergeFeatures.Execute();

            }
            // 保存
            Project.Current.SaveEditsAsync();
        }

        // 删除要素【按OID】
        public static void DelectFeature(FeatureLayer featureLayer, long oid)
        {
            var deleteFeatures = new EditOperation();

            // 删除要素中的某一行
            deleteFeatures.Delete(featureLayer, oid);
            deleteFeatures.Execute();
            // 保存
            Project.Current.SaveEditsAsync();
        }

        // 删除多个要素【按OID】
        public static void DelectFeatures(FeatureLayer featureLayer, List<long> oids)
        {
            var deleteFeatures = new EditOperation();
            // 删除要素中的某一行
            deleteFeatures.Delete(featureLayer, oids);
            deleteFeatures.Execute();
            // 保存
            Project.Current.SaveEditsAsync();
        }

        // 通过MapPoints创建点要素
        public static void CreatePointFromMapPoint(List<MapPoint> mapPoints, SpatialReference sr, string gdbPath, string fcName)
        {
            /// 创建点要素
            // 创建一个ShapeDescription
            var shapeDescription = new ShapeDescription(GeometryType.Point, sr)
            {
                HasM = false,
                HasZ = false
            };
            // 定义4个字段
            var pointX = new ArcGIS.Core.Data.DDL.FieldDescription("x坐标", FieldType.Double);
            var pointY = new ArcGIS.Core.Data.DDL.FieldDescription("y坐标", FieldType.Double);

            // 打开数据库gdb
            using (Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdbPath))))
            {
                // 收集字段列表
                var fieldDescriptions = new List<ArcGIS.Core.Data.DDL.FieldDescription>()
                {
                      pointX, pointY
                };
                // 创建FeatureClassDescription
                var fcDescription = new FeatureClassDescription(fcName, fieldDescriptions, shapeDescription);
                // 创建SchemaBuilder
                SchemaBuilder schemaBuilder = new SchemaBuilder(gdb);
                // 将创建任务添加到DDL任务列表中
                schemaBuilder.Create(fcDescription);
                // 执行DDL
                bool success = schemaBuilder.Build();

                // 创建要素并添加到要素类中
                using (FeatureClass featureClass = gdb.OpenDataset<FeatureClass>(fcName))
                {
                    /// 构建点要素
                    // 创建编辑操作对象
                    EditOperation editOperation = new EditOperation();
                    editOperation.Callback(context =>
                    {
                        // 获取要素定义
                        FeatureClassDefinition featureClassDefinition = featureClass.GetDefinition();
                        // 循环创建点
                        for (int i = 0; i < mapPoints.Count; i++)
                        {
                            // 创建RowBuffer
                            using RowBuffer rowBuffer = featureClass.CreateRowBuffer();
                            MapPoint pt = mapPoints[i];
                            // 写入字段值
                            rowBuffer["x坐标"] = pt.X;
                            rowBuffer["y坐标"] = pt.Y;

                            // 坐标
                            Coordinate2D newCoordinate = new Coordinate2D(pt.X, pt.Y);
                            // 创建点几何
                            MapPointBuilderEx mapPointBuilderEx = new(new Coordinate2D(pt.X, pt.Y));
                            // 给新添加的行设置形状
                            rowBuffer[featureClassDefinition.GetShapeField()] = mapPointBuilderEx.ToGeometry();

                            // 在表中创建新行
                            using Feature feature = featureClass.CreateRow(rowBuffer);
                            context.Invalidate(feature);      // 标记行为无效状态
                        }
                    }, featureClass);

                    // 执行编辑操作
                    editOperation.Execute();
                }
            }

            // 保存
            Project.Current.SaveEditsAsync();
        }

        // 添加字段
        public static void AddField(string targetPath, string fieldName, FieldType fieldType = FieldType.String, int filedLength = 255, string aliasName = "")
        {
            // 获取参数
            string gdbPath = targetPath.TargetPath();       // 数据库路径
            string fcName = targetPath.TargetName();     // 要素名称

            using Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdbPath)));

            // 获取待修改要素的【FeatureClassDefinition】
            FeatureClassDefinition featureClassDefinition = gdb.GetDefinition<FeatureClassDefinition>(fcName);
            // 获取【FeatureClassDescription】
            FeatureClassDescription featureClassDescription = new FeatureClassDescription(featureClassDefinition);

            // 别名
            if (aliasName == "") { aliasName = fieldName; }

            // 定义需要添加的字段
            FieldDescription description = new FieldDescription(fieldName, fieldType)
            {
                AliasName = aliasName,
                Length = filedLength,
            };

            // 获取所有字段名
            List<string> fields = new List<string>();
            foreach (var item in featureClassDescription.FieldDescriptions)
            {
                fields.Add(item.Name);
            }

            // 如果要添加的字段不重名，就将新字段添加到【FieldDescription】列表中
            List<FieldDescription> modifiedFieldDescriptions = new List<FieldDescription>(featureClassDescription.FieldDescriptions);
            if (!fields.Contains(fieldName))
            {
                modifiedFieldDescriptions.Add(description);
            }

            // 使用新添加的字段创建一个【FeatureClassDescription】
            FeatureClassDescription modifiedFeatureClassDescription = new FeatureClassDescription(featureClassDescription.Name, modifiedFieldDescriptions, featureClassDescription.ShapeDescription);

            SchemaBuilder schemaBuilder = new SchemaBuilder(gdb);

            // 更新要素
            schemaBuilder.Modify(modifiedFeatureClassDescription);
            schemaBuilder.Build();
        }

        // 通过Lyrx文件应用符号系统【唯一值】
        public static void ApplySymbol(string lyName, string fieldName, string lyrxPath)
        {
            // 获取FeatureLayer
            FeatureLayer featureLayer = lyName.TargetFeatureLayer();
            // 获取Lyrx图层
            LayerDocument lyrFile = new LayerDocument(lyrxPath);
            CIMLayerDocument cimLyrDoc = lyrFile.GetCIMLayerDocument();
            // 以唯一值方式获取CIMFeatureLayer
            CIMUniqueValueRenderer uvr = ((CIMFeatureLayer)cimLyrDoc.LayerDefinitions[0]).Renderer as CIMUniqueValueRenderer;
            // 修改一个参照字段
            uvr.Fields = new string[] { fieldName };
            // 应用渲染器
            featureLayer.SetRenderer(uvr);
        }

        // 通过Lyrx文件应用符号系统【唯一值】
        public static void ApplySymbol(FeatureLayer featureLayer, string fieldName, string lyrxPath)
        {
            // 获取Lyrx图层
            LayerDocument lyrFile = new LayerDocument(lyrxPath);
            CIMLayerDocument cimLyrDoc = lyrFile.GetCIMLayerDocument();
            // 以唯一值方式获取CIMFeatureLayer
            CIMUniqueValueRenderer uvr = ((CIMFeatureLayer)cimLyrDoc.LayerDefinitions[0]).Renderer as CIMUniqueValueRenderer;
            // 修改一个参照字段
            uvr.Fields = new string[] { fieldName };
            // 应用渲染器
            featureLayer.SetRenderer(uvr);
        }

        // 符号系统删除计数为0的值
        public static void Delete0uvClass(FeatureLayer featureLayer)
        {
            // 获取CIMUniqueValueRenderer
            CIMUniqueValueRenderer uvr = featureLayer.GetRenderer() as CIMUniqueValueRenderer;

            // 获取映射字段
            string mapFieldName = uvr.Fields.FirstOrDefault();
            // 获取字段值映射表
            List<string> listFieldValues = featureLayer.GetFieldValues(mapFieldName);

            CIMUniqueValueClass[] uvClasses = uvr.Groups[0].Classes;

            // 删除计数值为0的行
            uvr.Groups[0].Classes = uvClasses.Where(x => listFieldValues.Contains(x.Values[0].FieldValues[0].ToString())).ToArray();

            // 应用渲染器
            featureLayer.SetRenderer(uvr);
        }

        // 统计工具
        public static Dictionary<string, double> StatisticsValue(string lyName, string staField, string sumField = "Shape_Area", string sql = null, string total = "总计")
        {
            Dictionary<string, double> dict = new();

            // 获取Table
            Table table = lyName.TargetTable();

            // 设定筛选语句
            var queryFilter = new QueryFilter();
            queryFilter.WhereClause = sql;
            // 逐行游标
            using (RowCursor rowCursor = table.Search(queryFilter, false))
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        // 如果没有指定统计字段，则只统计总数
                        if (staField == "")
                        {
                            // 获取value
                            var va = row[sumField];
                            if (va is not null)
                            {
                                var value = double.Parse(va.ToString());
                                // 如果没有重复key值，则纳入dict
                                if (dict.Count == 0)
                                {
                                    dict.Add(total, value);
                                }
                                // 如果已有，则合并统计
                                else
                                {
                                    dict[total] += value;
                                }
                            }
                        }
                        // 如果指定统计字段，则按字段值统计
                        else
                        {
                            // 获取value
                            var key = row[staField];
                            var va = row[sumField];
                            if (key is not null && va is not null)
                            {
                                var value = double.Parse(va.ToString());
                                // 如果没有重复key值，则纳入dict
                                if (!dict.Keys.Contains(key.ToString()))
                                {
                                    dict.Add(key.ToString(), value);
                                }
                                // 如果已有，则合并统计
                                else
                                {
                                    dict[key.ToString()] += value;
                                }
                            }
                        }
                    }
                }
            }
            return dict;
        }

    }
}
