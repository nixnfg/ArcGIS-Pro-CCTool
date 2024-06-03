﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTool.Scripts.ToolManagers
{
    public class DataLib
    {
        // 村规导图DPI
        public static int vgDPI = 300;

        // 三调用地名称列表
        public static List<string> yd_sd = new List<string>() { "红树林地", "森林沼泽", "灌丛沼泽", "沼泽草地", "盐田", "沿海滩涂", "内陆滩涂", "沼泽地", "水田", "水浇地", "旱地", "果园", "茶园", "橡胶园", "其他园地", "可调整果园", "可调整茶园", "可调整橡胶园", "可调整其他园地", "乔木林地", "可调整乔木林地", "竹林地", "可调整竹林地", "灌木林地", "其他林地", "可调整其他林地", "天然牧草地", "人工牧草地", "可调整人工牧草地", "其他草地", "商业服务业设施用地", "物流仓储用地", "工业用地", "采矿用地", "城镇住宅用地", "农村宅基地", "机关团体新闻出版用地", "科教文卫用地", "高教用地", "公用设施用地", "公园与绿地", "广场用地", "特殊用地", "铁路用地", "轨道交通用地", "公路用地", "城镇村道路用地", "交通服务场站用地", "农村道路", "机场用地", "港口码头用地", "管道运输用地", "河流水面", "湖泊水面", "水库水面", "养殖坑塘", "可调整养殖坑塘", "坑塘水面", "沟渠", "干渠", "水工建筑用地", "冰川及永久积雪", "空闲地", "设施农用地", "田坎", "盐碱地", "沙地", "裸土地", "裸岩石砾地" };

        // 三调编码和全名对照表
        public static Dictionary<string, string> dic_sdAll = new Dictionary<string, string>()
        {
            {"00","湿地"},{"0303","红树林地"},{"0304","森林沼泽"},{"0306","灌丛沼泽"},{"0402","沼泽草地"},{"0603","盐田"},{"1105","沿海滩涂"},{"1106","内陆滩涂"},{"1108","沼泽地"},{"01","耕地"},{"0101","水田"},{"0102","水浇地"},{"0103","旱地"},{"02","种植园用地"},{"0201","果园"},{"0201K","可调整果园"},{"0202","茶园"},{"0202K","可调整茶园"},{"0203","橡胶园"},{"0203K","可调整橡胶园"},{"0204","其他园地"},{"0204K","可调整其他园地"},{"03","林地"},{"0301","乔木林地"},{"0301K","可调整乔木林地"},{"0302","竹林地"},{"0302K","可调整竹林地"},{"0305","灌木林地"},{"0307","其他林地"},{"0307K","可调整其他林地"},{"04","草地"},{"0401","天然牧草地"},{"0403","人工牧草地"},{"0403K","可调整人工牧草地"},{"0404","其他草地"},{"05","商业服务业用地"},{"05H1","商业服务业设施用地"},{"0508","物流仓储用地"},{"06","工矿用地"},{"0601","工业用地"},{"0602","采矿用地"},{"07","住宅用地"},{"0701","城镇住宅用地"},{"0702","农村宅基地"},{"08","公共管理与公共服务用地"},{"08H1","机关团体新闻出版用地"},{"08H2","科教文卫用地"},{"08H2A","高教用地"},{"0809","公用设施用地"},{"0810","公园与绿地"},{"0810A","广场用地"},{"09","特殊用地"},{"10","交通运输用地"},{"1001","铁路用地"},{"1002","轨道交通用地"},{"1003","公路用地"},{"1004","城镇村道路用地"},{"1005","交通服务场站用地"},{"1006","农村道路"},{"1007","机场用地"},{"1008","港口码头用地"},{"1009","管道运输用地"},{"11","水域及水利设施用地"},{"1101","河流水面"},{"1102","湖泊水面"},{"1103","水库水面"},{"1104","坑塘水面"},{"1104A","养殖坑塘"},{"1104K","可调整养殖坑塘"},{"1107","沟渠"},{"1107A","干渠"},{"1109","水工建筑用地"},{"1110","冰川及永久积雪"},{"12","其他土地"},{"1201","空闲地"},{"1202","设施农用地"},{"1203","田坎"},{"1204","盐碱地"},{"1205","沙地"},{"1206","裸土地"},{"1207","裸岩石砾地"},{"201","城市"},{"201A","城市独立工业仓储用地"},{"202","建制镇"},{"202A","建制镇"},{"203","村庄"},{"203A","村庄"},{"204","盐田及采矿用地"},{"205","特殊用地"},{"原来204","采矿用地"},{"原来205","风景名胜用地"}
        };

        // 【代码转名称】映射表
        public static Dictionary<string, string> dic_ydyh = new Dictionary<string, string>()
        {
            { "01", "耕地"},{ "0101", "水田"},{ "0102", "水浇地"},{ "0103", "旱地"},{ "02", "园地"},{ "0201", "果园"},{ "0202", "茶园"},{ "0203", "橡胶园"},{ "0204", "其他园地"},{ "03", "林地"},{ "0301", "乔木林地"},{ "0302", "竹林地"},{ "0303", "灌木林地"},{ "0304", "其他林地"},{ "04", "草地"},{ "0401", "天然牧草地"},{ "0402", "人工牧草地"},{ "0403", "其他草地"},{ "05", "湿地"},{ "0501", "森林沼泽"},{ "0502", "灌丛沼泽"},{ "0503", "沼泽草地"},{ "0504", "其他沼泽地"},{ "0505", "沿海滩涂"},{ "0506", "内陆滩涂"},{ "0507", "红树林地"},{ "06", "农业设施建设用地"},{ "0601", "乡村道路用地"},{ "060101", "村道用地"},{ "060102", "村庄内部道路用地"},{ "0602", "种植设施建设用地"},{ "0603", "畜禽养殖设施建设用地"},{ "0604", "水产养殖设施建设用地"},{ "07", "居住用地"},{ "0701", "城镇住宅用地"},{ "070101", "一类城镇住宅用地"},{ "070102", "二类城镇住宅用地"},{ "070103", "三类城镇住宅用地"},{ "0702", "城镇社区服务设施用地"},{ "0703", "农村宅基地"},{ "070301", "一类农村宅基地"},{ "070302", "二类农村宅基地"},{ "0704", "农村社区服务设施用地"},{ "08", "公共管理与公共服务用地"},{ "0801", "机关团体用地"},{ "0802", "科研用地"},{ "0803", "文化用地"},{ "080301", "图书与展览用地"},{ "080302", "文化活动用地"},{ "0804", "教育用地"},{ "080401", "高等教育用地"},{ "080402", "中等职业教育用地"},{ "080403", "中小学用地"},{ "080404", "幼儿园用地"},{ "080405", "其他教育用地"},{ "0805", "体育用地"},{ "080501", "体育场馆用地"},{ "080502", "体育训练用地"},{ "0806", "医疗卫生用地"},{ "080601", "医院用地"},{ "080602", "基层医疗卫生设施用地"},{ "080603", "公共卫生用地"},{ "0807", "社会福利用地"},{ "080701", "老年人社会福利用地"},{ "080702", "儿童社会福利用地"},{ "080703", "残疾人社会福利用地"},{ "080704", "其他社会福利用地"},{ "09", "商业服务业用地"},{ "0901", "商业用地"},{ "090101", "零售商业用地"},{ "090102", "批发市场用地"},{ "090103", "餐饮用地"},{ "090104", "旅馆用地"},{ "090105", "公用设施营业网点用地"},{ "0902", "商务金融用地"},{ "0903", "娱乐康体用地"},{ "090301", "娱乐用地"},{ "090302", "康体用地"},{ "0904", "其他商业服务业用地"},{ "10", "工矿用地"},{ "1001", "工业用地"},{ "100101", "一类工业用地"},{ "100102", "二类工业用地"},{ "100103", "三类工业用地"},{ "1002", "采矿用地"},{ "1003", "盐田"},{ "11", "仓储用地"},{ "1101", "物流仓储用地"},{ "110101", "一类物流仓储用地"},{ "110102", "二类物流仓储用地"},{ "110103", "三类物流仓储用地"},{ "1102", "储备库用地"},{ "12", "交通运输用地"},{ "1201", "铁路用地"},{ "1202", "公路用地"},{ "1203", "机场用地"},{ "1204", "港口码头用地"},{ "1205", "管道运输用地"},{ "1206", "城市轨道交通用地"},{ "1207", "城镇道路用地"},{ "1208", "交通场站用地"},{ "120801", "对外交通场站用地"},{ "120802", "公共交通场站用地"},{ "120803", "社会停车场用地"},{ "1209", "其他交通设施用地"},{ "13", "公用设施用地"},{ "1301", "供水用地"},{ "1302", "排水用地"},{ "1303", "供电用地"},{ "1304", "供燃气用地"},{ "1305", "供热用地"},{ "1306", "通信用地"},{ "1307", "邮政用地"},{ "1308", "广播电视设施用地"},{ "1309", "环卫用地"},{ "1310", "消防用地"},{ "1311", "干渠"},{ "1312", "水工设施用地"},{ "1313", "其他公用设施用地"},{ "14", "绿地与开敞空间用地"},{ "1401", "公园绿地"},{ "1402", "防护绿地"},{ "1403", "广场用地"},{ "15", "特殊用地"},{ "1501", "军事设施用地"},{ "1502", "使领馆用地"},{ "1503", "宗教用地"},{ "1504", "文物古迹用地"},{ "1505", "监教场所用地"},{ "1506", "殡葬用地"},{ "1507", "其他特殊用地"},{ "16", "留白用地"},{ "17", "陆地水域"},{ "1701", "河流水面"},{ "1702", "湖泊水面"},{ "1703", "水库水面"},{ "1704", "坑塘水面"},{ "1705", "沟渠"},{ "1706", "冰川及常年积雪"},{ "18", "渔业用海"},{ "1801", "渔业基础设施用海"},{ "1802", "增养殖用海"},{ "1803", "捕捞海域"},{ "19", "工矿通信用海"},{ "1901", "工业用海"},{ "1902", "盐田用海"},{ "1903", "固体矿产用海"},{ "1904", "油气用海"},{ "1905", "可再生能源用海"},{ "1906", "海底电缆管道用海"},{ "20", "交通运输用海"},{ "2001", "港口用海"},{ "2002", "航运用海"},{ "2003", "路桥隧道用海"},{ "21", "游憩用海"},{ "2101", "风景旅游用海"},{ "2102", "体休闲娱乐用海"},{ "22", "特殊用海"},{ "2201", "军事用海"},{ "2202", "其他特殊用海"},{ "23", "其他土地"},{ "2301", "空闲地"},{ "2302", "田坎"},{ "2303", "田间道"},{ "2304", "盐碱地"},{ "2305", "沙地"},{ "2306", "裸土地"},{ "2307", "裸岩石砾地"},{ "24", "其他海域"}
        };

        // Excel表格序号对照表
        public static Dictionary<int, string> excelPairs = new Dictionary<int, string>()
        {
            {1, "A" },{2, "B" },{3, "C" },{4, "D" },{5, "E" },{6, "F" },{7, "G" },{8, "H" },{9, "I" },{10, "J" },{11, "K" },{12, "L" },{13, "M" },{14, "N" },{15, "O" },{16, "P" },{17, "Q" },{18, "R" },{19, "S" },{20, "T" },{21, "U" },{22, "V" },{23, "W" },{24, "X" },{25, "Y" },{26, "Z" },
        };
    }
}