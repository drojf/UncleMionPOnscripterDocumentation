﻿using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html;

namespace onscripter_documentation
{
    class Program
    {
        static IElement GenerateElementFromFunctionEntry(IHtmlDocument doc, FunctionEntry fe)
        {
            IElement functionEntryRoot = doc.CreateElement("div");
            functionEntryRoot.ClassName = "FunctionEntry";

            //Create function link
            var idLink = doc.CreateElement("a");
            idLink.Id = fe.id;
            functionEntryRoot.AppendChild(idLink);

            //Create function name
            var functionName = doc.CreateElement("h2");
            functionName.TextContent = fe.headerInformation.wordName;
            functionEntryRoot.AppendChild(functionName);

            //Create function description
            var functionDescription = doc.CreateElement("div");
            functionDescription.ClassName = "FunctionDescription";
            functionDescription.InnerHtml = fe.functionDescription.descriptionHTML;
            functionEntryRoot.AppendChild(functionDescription);

            return functionEntryRoot;
        }

        static string GenerateDocument(string templateFilePath, List<FunctionEntry> functionEntries)
        {
            var document = new HtmlParser().ParseDocument(File.ReadAllText(templateFilePath, Encoding.UTF8));

            // Insert generated document header information here
            var p = document.CreateElement("p");
            p.TextContent = "Insert generated document header information here";
            document.Body.AppendChild(p);

            foreach (FunctionEntry functionEntry in functionEntries)
            {
                document.Body.AppendChild(GenerateElementFromFunctionEntry(document, functionEntry));
            }

            var sw = new StringWriter();
            document.ToHtml(sw, new PrettyMarkupFormatter());
            return sw.ToString();
        }

        static void Main(string[] args)
        {
            string templateFilePath = "entire_document_template.html";

            string htmlPath = @"..\..\..\..\WaybackArchive\NScripter API Reference [compiled by senzogawa, translated_annotated_XML-ized by Mion (incorporates former translation by Seung 'gp32' Park)].html";

            //read in from html file
            string htmltext = File.ReadAllText(htmlPath);

            // Create a new parser front-end (can be re-used)
            var parser = new HtmlParser();

            //Just get the DOM representation
            var document = parser.ParseDocument(htmltext);

            //look for <div id="MAIN">
            var functionDetailedDocumentation = document.GetElementById("MAIN");

            var childIter = functionDetailedDocumentation.Children.GetEnumerator();

            var globalClasses = new HashSet<string>();

            List<FunctionEntry> functionEntries = new List<FunctionEntry>(); 
            while (true)
            {
                FunctionEntry ent = ParsingFunctions.ParseOneFunctionEntry(childIter);
                if(ent == null)
                {
                    break;
                }
                else
                {
                    functionEntries.Add(ent);
                }
            }

            string output = GenerateDocument(templateFilePath, functionEntries);
            Console.WriteLine(output);
            File.WriteAllText("GeneratedDocumentation.html", output);

            //write out function entries as json
            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter sw = new StreamWriter(@"onscripter_documentation.json"))
            using (JsonWriter writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented } )
            {
                serializer.Serialize(writer, functionEntries);
            }

            Util.pauseExit(0);
        }
    }
}
