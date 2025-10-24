using GemBox.Document;
using GemBox.Document.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.MotorTest.Model
{
    internal class MotorTestReportModel
    {
        public MotorTestReportModel(ThreeAxisTestModel testModel, List<List<MotorTestMessage>> motorTestMessages)
        {
            _testModel = testModel;
            _motorTestMessages = motorTestMessages;
        }
        List<List<MotorTestMessage>> _motorTestMessages;
        public ThreeAxisTestModel _testModel { get; set; } 
        private void add( )
        {
            // 激活GemBox.Document免费版（需先在官网获取免费密钥：https://www.gemboxsoftware.com/document/free-version）
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");

            // 创建Word文档
            DocumentModel doc = new DocumentModel();

            // ========== 1. 添加标题 ==========
            doc.Sections.Add(new Section(doc)
            {
                Blocks =
                {
                    new Paragraph(doc)
                    {
                        Inlines = { new Run(doc, "电机测试报告") { CharacterFormat = { Size = 20, Bold = true } } },
                        ParagraphFormat = { Alignment = GemBox.Document.HorizontalAlignment.Center }
                    }
                }
            });

            // ========== 2. 添加正文和表格 ==========
           
            Section section = doc.Sections[0];
            section.Blocks.Add(new Paragraph(doc, $"生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}"));
            section.Blocks.Add(new Paragraph(doc, "测试数据统计："));
            for (int i = 0; i < _testModel.Motors.Count; i ++)
            {
                // 插入表格（3行3列）
                Table table = new Table(doc, 2, 6);
                int count = _motorTestMessages[i].Count;
                table.TableFormat.Borders.SetBorders(MultipleBorderTypes.All, GemBox.Document.BorderStyle.Single, GemBox.Document.Color.Black, 1);
                // 表头
                table.Rows[0].Cells[0].Blocks.Add(new Paragraph(doc, "ID"));
                table.Rows[0].Cells[1].Blocks.Add(new Paragraph(doc, "测试项"));
                table.Rows[0].Cells[2].Blocks.Add(new Paragraph(doc, "测试结果"));
                table.Rows[0].Cells[3].Blocks.Add(new Paragraph(doc, "测量值"));
                table.Rows[0].Cells[4].Blocks.Add(new Paragraph(doc, "标准值"));
                table.Rows[0].Cells[5].Blocks.Add(new Paragraph(doc, "说明"));
                for (int j = 1; j < count; j++)
                {
                    // 内容行
                    table.Rows[j].Cells[0].Blocks.Add(new Paragraph(doc, $"{_motorTestMessages}"));
                    table.Rows[j].Cells[1].Blocks.Add(new Paragraph(doc, "电机控制测试"));
                    table.Rows[j].Cells[2].Blocks.Add(new Paragraph(doc, "合格"));
                    table.Rows[j].Cells[3].Blocks.Add(new Paragraph(doc, "移动距离15851:"));
                    table.Rows[j].Cells[4].Blocks.Add(new Paragraph(doc, "正常"));
                    table.Rows[j].Cells[5].Blocks.Add(new Paragraph(doc, "无"));
                    section.Blocks.Add(table);
                }
                
            }
            // 插入图片（关键修正：使用 Picture.Load 加载图片）
            string imagePath = @"C:\Users\ADMIN\Pictures\23-04-26-09-43-02.0632.png"; // 替换为你的图片路径
            if (File.Exists(imagePath))
            {
                // 添加图片说明文字
                section.Blocks.Add(new Paragraph(doc, "测试结果图表："));

                // 加载图片（正确方式：通过 Picture.Load 方法）
                Picture picture = new Picture(doc, imagePath, 400, 300);


                // 将图片添加到段落中，再插入文档
                Paragraph imagePara = new Paragraph(doc);
                imagePara.Inlines.Add(picture); // 图片本质是Inline元素，需添加到Inlines集合
                section.Blocks.Add(imagePara);
            }
            else
            {
                section.Blocks.Add(new Paragraph(doc, "（提示：图片文件未找到，无法插入）"));
            }

            // 保存文档
            string savePath = Path.Combine(Environment.CurrentDirectory, "电机测试报告.docx");
            doc.Save(savePath);
            Console.WriteLine($"报告已保存至：{savePath}");
        }
    
    }
}
