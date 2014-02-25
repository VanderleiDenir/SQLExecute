//using CarlosAg.ExcelXmlWriter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ScriptRunner
{
    //public class ExcelWriter : IExcelWriter
    //{
    //    public void WriteToExcel(List<ScriptDetails> scriptDetails, string totalTime)
    //    {
    //        string str1 = DateTime.Now.ToString("dd_MM_yyyy_hh-mm-ss");
    //        string fileName = "ScriptReport_" + str1 + ".xls";
    //        string str2 = "ScriptReport_" + str1 + ".csv";
    //        Workbook workbook = new Workbook();
    //        workbook.get_ExcelWorkbook().set_ActiveSheetIndex(1);
    //        workbook.get_ExcelWorkbook().set_WindowTopX(100);
    //        workbook.get_ExcelWorkbook().set_WindowTopY(200);
    //        workbook.get_ExcelWorkbook().set_WindowHeight(7000);
    //        workbook.get_ExcelWorkbook().set_WindowWidth(8000);
    //        workbook.get_Properties().set_Title("Script Run Details");
    //        workbook.get_Properties().set_Created(DateTime.Now);
    //        WorksheetStyle worksheetStyle1 = workbook.get_Styles().Add("HeaderStyle");
    //        worksheetStyle1.get_Font().set_FontName("Tahoma");
    //        worksheetStyle1.get_Font().set_Size(14);
    //        worksheetStyle1.get_Font().set_Bold(true);
    //        worksheetStyle1.get_Alignment().set_Horizontal((StyleHorizontalAlignment)1);
    //        worksheetStyle1.get_Font().set_Color("White");
    //        worksheetStyle1.get_Interior().set_Color("Blue");
    //        worksheetStyle1.get_Interior().set_Pattern((StyleInteriorPattern)8);
    //        WorksheetStyle worksheetStyle2 = workbook.get_Styles().Add("Default");
    //        worksheetStyle2.get_Font().set_FontName("Tahoma");
    //        worksheetStyle2.get_Font().set_Size(10);
    //        Worksheet worksheet = workbook.get_Worksheets().Add("Script Data");
    //        worksheet.get_Table().get_Columns().Add(new WorksheetColumn(200));
    //        worksheet.get_Table().get_Columns().Add(new WorksheetColumn(100));
    //        worksheet.get_Table().get_Columns().Add(new WorksheetColumn(100));
    //        worksheet.get_Table().get_Columns().Add(new WorksheetColumn(1000));
    //        WorksheetRow worksheetRow1 = worksheet.get_Table().get_Rows().Add();
    //        worksheetRow1.get_Cells().Add(new WorksheetCell("File Name", "HeaderStyle"));
    //        worksheetRow1.get_Cells().Add(new WorksheetCell("Time Taken to Run", "HeaderStyle"));
    //        worksheetRow1.get_Cells().Add(new WorksheetCell("Status", "HeaderStyle"));
    //        worksheetRow1.get_Cells().Add(new WorksheetCell("Remarks", "HeaderStyle"));
    //        foreach (ScriptDetails scriptDetails1 in scriptDetails)
    //        {
    //            WorksheetRow worksheetRow2 = worksheet.get_Table().get_Rows().Add();
    //            worksheetRow2.get_Cells().Add(scriptDetails1.filename);
    //            worksheetRow2.get_Cells().Add(scriptDetails1.timetaken);
    //            worksheetRow2.get_Cells().Add(scriptDetails1.status);
    //            StringBuilder stringbuilder = new StringBuilder();
    //            scriptDetails1.remarks.ForEach((Action<string>)(y => stringbuilder.AppendLine(y)));
    //            worksheetRow2.get_Cells().Add(((object)stringbuilder).ToString());
    //        }
    //        worksheet.get_Table().get_Rows().Add();
    //        WorksheetRow worksheetRow3 = worksheet.get_Table().get_Rows().Add();
    //        worksheetRow3.get_Cells().Add("TotalTime");
    //        worksheetRow3.get_Cells().Add(totalTime);
    //        workbook.Save(fileName);
    //        try
    //        {
    //            Process.Start(fileName);
    //        }
    //        catch
    //        {
    //        }
    //    }
    //}
}
