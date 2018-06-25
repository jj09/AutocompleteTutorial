﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.Configuration;

namespace AutocompleteTutorial.Controllers
{
    public class HomeController : Controller
    {
        private static SearchServiceClient _searchClient;
        private static ISearchIndexClient _indexClient;
        private static string IndexName = "nycjobs";

        public static string errorMessage;

        private void InitSearch()
        {
                string searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
                string apiKey = ConfigurationManager.AppSettings["SearchServiceApiKey"];

                // Create a reference to the NYCJobs index
                _searchClient = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));
                _indexClient = _searchClient.Indexes.GetClient(IndexName);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Suggest(bool highlights, string term)
        {
            InitSearch();

            // Call suggest API and return results
            SuggestParameters sp = new SuggestParameters()
            {
                UseFuzzyMatching = false,
                Top = 5,
            };

            if (highlights)
            {
                sp.HighlightPreTag = "<b>";
                sp.HighlightPostTag = "</b>";
            }

            DocumentSuggestResult resp = _indexClient.Documents.Suggest(term, "sg", sp);

            // Convert the suggest query results to a list that can be displayed in the client.
            List<string> suggestions = resp.Results.Select(x => x.Text).Distinct().ToList();
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = suggestions
            };
        }

        public ActionResult AutoComplete(string term)
        {
            InitSearch();
            // Call autocomplete API and return results
            //AutocompleteParameters sp = new AutocompleteParameters()
            //{
            //    AutocompleteMode = mode,
            //    UseFuzzyMatching = fuzzy,
            //    Top = top,
            //    MinimumCoverage = 80
            //};
            //AutocompleteResult resp = _indexClient.Documents.Autocomplete(term, "sg", sp);

            //// Conver the Suggest results to a list that can be displayed in the client.
            //List<string> autocomplete = resp.Results.Select(x => x.Text).Distinct().ToList();
            //return new JsonResult
            //{
            //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            //    Data = autocomplete
            //};
            return null;
        }

        public ActionResult Both(string term)
        {
            InitSearch();

            // Call both the suggest and autocomplete API and return results
            SuggestParameters sp = new SuggestParameters()
            {
                UseFuzzyMatching = false,
                Top = 5
            };
            DocumentSuggestResult resp1 = _indexClient.Documents.Suggest(term, "sg", sp);
            var result = resp1.Results.Select(x => x.Text).Distinct().ToList().Select(x => new { label = x, category = "Suggestions" }).ToList();

            //AutocompleteParameters sp = new AutocompleteParameters()
            //{
            //    AutocompleteMode = mode,
            //    UseFuzzyMatching = fuzzy,
            //    Top = top,
            //    MinimumCoverage = 80
            //};
            //AutocompleteResult resp2 = _indexClient.Documents.Autocomplete(term, "sg", sp);

            // Convert the suggest query results to a list that can be displayed in the client.
            //result.AddRange(resp1.Results.Select(x => x.Text).Distinct().ToList().Select(x => new { label = x, category = "Autocomplete" }).ToList());

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = result
            };
        }

        public ActionResult Facets()
        {
            InitSearch();

            // Call suggest API and return results
            SearchParameters sp = new SearchParameters()
            {
                Facets = new List<string> { "agency,count:500" },
            };


            DocumentSearchResult resp = _indexClient.Documents.Search("*", sp);

            // Convert the suggest query results to a list that can be displayed in the client.

            List<string> facets = resp.Facets["agency"].Select(x => x.Value.ToString()).ToList();
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = facets
            };
        }
    }
}