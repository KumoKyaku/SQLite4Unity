///无法找到命名空间请检查unityVS插件是否正确安装
using SyntaxTree.VisualStudio.Unity.Bridge;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace UnityEditor.Support
{
    /// <summary>
    /// 允许你得UNITY项目识别数据集设计器
    /// <para>以xml方式打开.xsd文件，找到 DataSource->Connections下的所有Connection元素，</para>
    /// <para>设置ConnectionStringObject属性为连接字符串，路径为绝对路径。</para>
    ///       例如：ConnectionStringObject="Data Source=C:\Users\admin\Desktop\test.db"
    /// <para>设置IsAppSettingsProperty="false"</para>
    /// <para>XML to open the.Xsd file, find all the elements of the DataSource->Connections Connection,
    /// </para><para>
    ///       Set the ConnectionStringObject Attribute to the connection string, which is an absolute path.
    ///       Such as: ConnectionStringObject= "Data Source=C:\\Users\\admin\\Desktop\\test.db"</para>
    /// <para>Set IsAppSettingsProperty = "false"</para>
    /// </summary>
    [InitializeOnLoad]
    public static class DataSetDesignerSupport
    {
        static DataSetDesignerSupport()
        {
            //string[] datasetsupport = new string[] { "xsd", "xss", "xsc" };

            //List<string> add = new List<string>();
            //foreach (var item in datasetsupport)
            //{
            //    if (EditorSettings.projectGenerationUserExtensions.Contains(item))
            //    {
            //        continue;
            //    }
            //    else
            //    {
            //        add.Add(item);
            //    }
            //}
            //EditorSettings.projectGenerationUserExtensions =
            //    EditorSettings.projectGenerationUserExtensions.Concat(add).ToArray();

            ProjectFilesGenerator.ProjectFileGeneration +=
        new FileGenerationHandler(Post);
        }

        private static string Post(string fileName, string fileContent)
        {
            var files = AssetDatabase.GetAllAssetPaths();

            var project = CheckString(fileName);

            XDocument CON = XDocument.Parse(fileContent);
            foreach (var fileProjectPathName in files)
            {
                if (!fileProjectPathName.Contains("Assets"))
                {
                    continue;
                }

                ///后缀名
                var extension = Path.GetExtension(fileProjectPathName);
                if (extension == ".xsd" || extension == "xsd")
                {
                    var res = CheckFile(fileProjectPathName);
                    if (project.IsEditor != res.IsEditor)
                    {
                        //Todo
                        continue;
                    }

                    if (project.IsPlugins != res.IsPlugins)
                    {
                        //Todo
                        continue;
                    }

                    ///转换 /
                    string fileProjectPathNameFIX = fileProjectPathName.Replace('/', '\\');

                    ///xss文件路径名
                    string xssName = Path.ChangeExtension(fileProjectPathNameFIX, ".xss");
                    ///xsc文件路径名
                    string xscName = Path.ChangeExtension(fileProjectPathNameFIX, ".xsc");

                    ///对应的数据集脚本名字
                    string scName = Path.ChangeExtension(fileProjectPathNameFIX, ".cs");
                    ///对应的数据集设计器脚本名字
                    string designercsName = Path.ChangeExtension(fileProjectPathNameFIX, "Designer.cs");

                    ///xsd文件短名
                    string shortFileName = Path.GetFileName(fileProjectPathNameFIX);

                    ///获取所以元素
                    var elements = CON.Descendants();

                    ///cs文件引用元素  元素名为Compile
                    var csDes = from a in elements
                                where a.Name.LocalName == "Compile" && a.Attribute("Include") != null
                                && (a.Attribute("Include").Value == scName ||
                                a.Attribute("Include").Value == designercsName)
                                select a;

                    foreach (var cs in csDes)
                    {
                        var csName = cs.Attribute("Include").Value;
                        if (csName == designercsName)
                        {
                            ///如果是设计器 额外加入两个元素描述
                            cs.Add(new XElement(XName.Get("AutoGen", cs.Name.NamespaceName)) { Value = "True" });
                            cs.Add(new XElement(XName.Get("DesignTime", cs.Name.NamespaceName)) { Value = "True" });
                        }
                        cs.Add(new XElement(XName.Get("DependentUpon", cs.Name.NamespaceName))
                        { Value = shortFileName });
                    }

                    /////设计器文件引用元素 元素名为None
                    //var desingerDes = from a in elements
                    //             where a.Name.LocalName == "None" && a.Attribute("Include") != null
                    //             && (a.Attribute("Include").Value == fileProjectPathNameFIX
                    //             || a.Attribute("Include").Value == xssName
                    //             || a.Attribute("Include").Value == xscName)
                    //             select a;

                    //foreach (var desingerEle in desingerDes)
                    //{
                    //    string desinfileName = desingerEle.Attribute("Include").Value;
                    //    if (desinfileName == fileProjectPathNameFIX)
                    //    {
                    //        desingerEle.Add(new XElement(XName.Get("SubType", desingerEle.Name.NamespaceName)) { Value = "Designer" });
                    //        desingerEle.Add(new XElement(XName.Get("Generator", desingerEle.Name.NamespaceName)) { Value = "MSDataSetGenerator" });
                    //        desingerEle.Add(new XElement(XName.Get("LastGenOutput", desingerEle.Name.NamespaceName))
                    //                                    { Value = Path.GetFileName(designercsName) });
                    //    }
                    //    else
                    //    {
                    //        desingerEle.Add(new XElement(XName.Get("DependentUpon", desingerEle.Name.NamespaceName))
                    //                                    { Value = Path.GetFileName(fileProjectPathNameFIX) });
                    //    }
                    //}


                    XElement xsdGroup = new XElement(XName.Get("ItemGroup", CON.Root.Name.NamespaceName));

                    XElement xsd = new XElement(XName.Get("None", xsdGroup.Name.NamespaceName));
                    xsd.Add(new XAttribute("Include", fileProjectPathNameFIX));
                    xsd.Add(new XElement(XName.Get("SubType", xsd.Name.NamespaceName)) { Value = "Designer" });
                    xsd.Add(new XElement(XName.Get("Generator", xsd.Name.NamespaceName)) { Value = "MSDataSetGenerator" });
                    xsd.Add(new XElement(XName.Get("LastGenOutput", xsd.Name.NamespaceName))
                    { Value = Path.GetFileName(designercsName) });
                    xsdGroup.Add(xsd);

                    XElement xss = new XElement(XName.Get("None", xsdGroup.Name.NamespaceName));
                    xss.Add(new XAttribute("Include", xssName));
                    xss.Add(new XElement(XName.Get("DependentUpon", xss.Name.NamespaceName))
                    { Value = Path.GetFileName(fileProjectPathNameFIX) });
                    xsdGroup.Add(xss);

                    XElement xsc = new XElement(XName.Get("None", xsdGroup.Name.NamespaceName));
                    xsc.Add(new XAttribute("Include", xscName));
                    xsc.Add(new XElement(XName.Get("DependentUpon", xsc.Name.NamespaceName))
                    { Value = Path.GetFileName(fileProjectPathNameFIX) });
                    xsdGroup.Add(xsc);

                    var last = CON.Descendants(XName.Get("ItemGroup", xsd.Name.NamespaceName)).LastOrDefault();
                    if (last != null)
                    {
                        last.AddAfterSelf(xsdGroup);
                    }
                    else
                    {
                        CON.Descendants(XName.Get("Project", xsd.Name.NamespaceName)).FirstOrDefault().Add(xsdGroup);
                    }
                }
            }

            var writer = new Utf8StringWriter();
            CON.Save(writer);
            return writer.ToString();
        }

        class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        private static FileType CheckFile(string fileProjectPathName)
        {
            var fileName = Path.GetFileName(fileProjectPathName);
            var projectPath = fileProjectPathName.Substring(0, fileProjectPathName.Length - fileName.Length);
            FileType res = CheckString(projectPath);
            return res;
        }

        private static FileType CheckString(string projectPath)
        {
            FileType res = new FileType();
            if (projectPath.Contains("Editor"))
            {
                res.IsEditor = true;
            }
            else
            {
                res.IsEditor = false;
            }

            if (projectPath.Contains("Plugins"))
            {
                res.IsPlugins = true;
            }
            else
            {
                res.IsPlugins = false;
            }

            return res;
        }

        class FileType
        {
            public bool IsEditor { get; internal set; }
            public bool IsPlugins { get; internal set; }
        }
    }
}