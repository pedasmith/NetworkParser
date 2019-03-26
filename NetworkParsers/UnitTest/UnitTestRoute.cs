using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkParsers;

namespace UnitTest
{
    [TestClass]
    public class UnitTestRoute
    {
        [TestMethod]
        public void TestRouteSimple()
        {
            var r = new Route("/users/{id}/action", "METHODNAME");
            var found = r.Match("/users/person/action");

            Assert.AreNotEqual(null, found, "/users/person/action matches /users/{id}/action");
            Assert.AreEqual(1, found.Keys.Count, "Got one value");
            Assert.AreEqual(true, found.ContainsKey("id"), "Got an ID value");
            Assert.AreEqual("person", found["id"], "ID value is person");


            var notFoundTooLong = r.Match("/users/person/action/subaction");
            Assert.AreEqual(null, notFoundTooLong, "/users/person/action/subaction is too long to match /users/{id}/action");

            var notFoundTooShort = r.Match("/users/person");
            Assert.AreEqual(null, notFoundTooShort, "/users/person is too short to match /users/{id}/action");

            var notFoundWrongVerb = r.Match("/users/person/notaction");
            Assert.AreEqual(null, notFoundWrongVerb, "/users/person/notaction has wrong verb to match /users/{id}/action");
        }

        [TestMethod]
        public void TestRouteNoKeys()
        {
            var r = new Route("/users/id/action", "METHODNAME");
            var found = r.Match("/users/id/action");

            Assert.AreNotEqual(null, found, "/users/id/action matches /users/id/action");
            Assert.AreEqual(0, found.Keys.Count, "Got zero values");


            var notFoundWrong = r.Match("/users/person/action");
            Assert.AreEqual(null, notFoundWrong, "/users/person/action is no match for /users/id/action");

            var notFoundTooLong = r.Match("/users/person/action/subaction");
            Assert.AreEqual(null, notFoundTooLong, "/users/person/action/subaction is too long to match /users/id/action");

            var notFoundTooShort = r.Match("/users/person");
            Assert.AreEqual(null, notFoundTooShort, "/users/person is too short to match /users/id/action");

            var notFoundWrongVerb = r.Match("/users/person/notaction");
            Assert.AreEqual(null, notFoundWrongVerb, "/users/person/notaction has wrong verb to match /users/id/action");
        }

        [TestMethod]
        public void TestRouteTableSimple()
        {
            var t = new RouteTable();
            t.AddRoute("/user/{id}", "ID");
            t.AddRoute("/user/{id}/action", "ACTION");
            t.AddRoute("/user/{id}/action/{level}", "LEVEL");
            t.AddRoute("", "DEFAULT");

            var rdefault = t.Find("/not/a/route");
            Assert.AreNotEqual(null, rdefault, "Match a default route");

            var rid = t.Find("/user/person");
            var raction = t.Find("/user/person/action");
            var rlevel = t.Find("/user/person/action/verbose");

            Assert.AreNotEqual(null, rid, "Match the id route");
            Assert.AreNotEqual(null, raction, "Match the action route");
            Assert.AreNotEqual(null, rlevel, "Match the level route");

            Assert.AreEqual("ID", rid.Route.Data, $"Id route is id ({rid.Route.Data})");
            Assert.AreEqual("ACTION", raction.Route.Data, $"Action route is id ({raction.Route.Data})");
            Assert.AreEqual("LEVEL", rlevel.Route.Data, $"Level route is id ({rlevel.Route.Data})");

            Assert.AreEqual("person", rid.Values["id"], $"Id route id is person");
            Assert.AreEqual(1, raction.Values.Count, $"action matched 1");
            Assert.AreEqual("person", rlevel.Values["id"], $"Level id is person");
            Assert.AreEqual("verbose", rlevel.Values["level"], $"Level level is verbose");
            Assert.AreEqual(2, rlevel.Values.Count, $"action route matched 2");
        }
    }
}
