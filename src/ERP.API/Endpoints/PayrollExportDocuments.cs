using System.IO.Compression;
using System.Security;
using System.Text;
using System.Xml;
using ERP.Application.Features.Payroll.Contracts;

namespace ERP.API.Endpoints;

/// <summary>Creates export documents from an already-persisted payroll projection; it never calculates payroll values.</summary>
internal static class PayrollExportDocuments
{
    public static byte[] Excel(PayrollPeriodSnapshot payroll)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            Add(archive, "[Content_Types].xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>");
            Add(archive, "_rels/.rels", "<?xml version=\"1.0\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
            Add(archive, "xl/workbook.xml", "<?xml version=\"1.0\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"Planilla\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
            Add(archive, "xl/_rels/workbook.xml.rels", "<?xml version=\"1.0\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>");
            Add(archive, "xl/worksheets/sheet1.xml", Worksheet(payroll));
        }
        return stream.ToArray();
    }

    public static byte[] Payslips(PayrollPeriodSnapshot payroll)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            foreach (var row in payroll.Resultados)
                Add(archive, $"boleta-{row.EmpleadoId}.pdf", Pdf(payroll, row));
        }
        return stream.ToArray();
    }

    private static string Worksheet(PayrollPeriodSnapshot payroll)
    {
        var rows = new List<IReadOnlyList<string>> { new string[] { "Empleado", "Departamento", "Salario base", "Horas extra", "Total bruto", "AFP", "ONP", "Descuentos", "Neto", "Provision gratificacion", "Provision CTS", "Costo total" } };
        rows.AddRange(payroll.Resultados.Select(x => (IReadOnlyList<string>)new string[] { x.Nombre, x.Departamento, x.SalarioBase.ToString("0.00"), x.HorasExtraMonto.ToString("0.00"), x.TotalBruto.ToString("0.00"), x.Afp.ToString("0.00"), x.Onp.ToString("0.00"), x.TotalDescuentos.ToString("0.00"), x.TotalNeto.ToString("0.00"), x.ProvisionGratificacion.ToString("0.00"), x.ProvisionCts.ToString("0.00"), x.CostoTotal.ToString("0.00") }));
        rows.Add(new string[] { "Totales", "", "", "", payroll.TotalBruto.ToString("0.00"), "", "", payroll.TotalDescuentos.ToString("0.00"), payroll.TotalNeto.ToString("0.00"), payroll.TotalProvisionGratificacion.ToString("0.00"), payroll.TotalProvisionCts.ToString("0.00"), payroll.CostoPlanilla.ToString("0.00") });
        var xml = new StringBuilder("<?xml version=\"1.0\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
        for (var r = 0; r < rows.Count; r++)
        {
            xml.Append($"<row r=\"{r + 1}\">");
            foreach (var value in rows[r])
            {
                if (decimal.TryParse(value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out _))
                    xml.Append("<c t=\"n\"><v>").Append(value).Append("</v></c>");
                else
                    xml.Append("<c t=\"inlineStr\"><is><t>").Append(SecurityElement.Escape(value)).Append("</t></is></c>");
            }
            xml.Append("</row>");
        }
        return xml.Append("</sheetData></worksheet>").ToString();
    }

    private static string Pdf(PayrollPeriodSnapshot payroll, PayrollEmployeeResultSnapshot row)
    {
        var text = EscapePdf($"Boleta {payroll.Periodo} - {row.Nombre} | Bruto S/ {row.TotalBruto:0.00} | Descuentos S/ {row.TotalDescuentos:0.00} | Neto S/ {row.TotalNeto:0.00}");
        var content = $"BT /F1 10 Tf 40 740 Td ({text}) Tj ET\n";
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.ASCII, 1024, true) { NewLine = "\n" };
        writer.Write("%PDF-1.4\n%"); writer.Flush(); stream.WriteByte(0xE2); stream.WriteByte(0xE3); stream.WriteByte(0xCF); stream.WriteByte(0xD3); writer.Write('\n');
        var offsets = new List<long> { 0 };
        WriteObject(writer, stream, offsets, 1, "<</Type/Catalog/Pages 2 0 R>>");
        WriteObject(writer, stream, offsets, 2, "<</Type/Pages/Count 1/Kids[3 0 R]>>");
        WriteObject(writer, stream, offsets, 3, "<</Type/Page/Parent 2 0 R/MediaBox[0 0 612 792]/Resources<</Font<</F1 4 0 R>>>>/Contents 5 0 R>>");
        WriteObject(writer, stream, offsets, 4, "<</Type/Font/Subtype/Type1/BaseFont/Helvetica>>");
        WriteObject(writer, stream, offsets, 5, $"<</Length {Encoding.ASCII.GetByteCount(content)}>>\nstream\n{content}endstream");
        writer.Flush(); var xref = stream.Position; writer.Write($"xref\n0 {offsets.Count}\n0000000000 65535 f \n"); foreach (var offset in offsets.Skip(1)) writer.Write($"{offset:D10} 00000 n \n"); writer.Write($"trailer\n<</Size {offsets.Count}/Root 1 0 R>>\nstartxref\n{xref}\n%%EOF\n"); writer.Flush(); return Encoding.ASCII.GetString(stream.ToArray());
    }

    private static void WriteObject(StreamWriter writer, MemoryStream stream, ICollection<long> offsets, int id, string body) { writer.Flush(); offsets.Add(stream.Position); writer.Write($"{id} 0 obj\n{body}\nendobj\n"); }
    private static string EscapePdf(string text) => text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    private static void Add(ZipArchive archive, string name, string content)
    { using var writer = new StreamWriter(archive.CreateEntry(name).Open(), new UTF8Encoding(false)); writer.Write(content); }
}
