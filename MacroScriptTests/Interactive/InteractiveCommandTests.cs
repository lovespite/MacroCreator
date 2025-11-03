namespace MacroScript.Interactive.Tests
{
    [TestClass()]
    public class InteractiveCommandTests
    {
        [TestMethod()]
        public void ParseTest0()
        {
            var cmd1 = InteractiveCommand.Parse($"run");
            Assert.AreEqual("run", cmd1.PrimaryCommand);
            Assert.AreEqual(0, cmd1.Args.Length);
        }

        [TestMethod()]
        public void ParseTest0_1()
        {
            var cmd1 = InteractiveCommand.Parse($"run \"\"");
            Assert.AreEqual("run", cmd1.PrimaryCommand);
            Assert.AreEqual(1, cmd1.Args.Length);
            Assert.AreEqual(string.Empty, cmd1.Args[0]);
        }

        [TestMethod()]
        public void ParseTest1()
        {
            string[] args = ["arg1", "arg2", "arg3"];
            var cmd1 = InteractiveCommand.Parse($"run {string.Join(' ', args)}");
            Assert.AreEqual("run", cmd1.PrimaryCommand);
            CollectionAssert.AreEqual(args, cmd1.Args);
        }

        [TestMethod()]
        public void ParseTest2()
        {
            string arg1 = "arg1 arg1-2";
            string arg2 = "arg2";
            string arg3 = "arg3 arg3-1";
            string arg4 = "arg4 arg4-1";

            var cmd1 = InteractiveCommand.Parse($"run \"{arg1}\" {arg2} '{arg3}' `{arg4}`");
            Assert.AreEqual("run", cmd1.PrimaryCommand);
            Assert.AreEqual(4, cmd1.Args.Length);

            Assert.AreEqual(arg1, cmd1.Args[0]);
            Assert.AreEqual(arg2, cmd1.Args[1]);
            Assert.AreEqual(arg3, cmd1.Args[2]);
            Assert.AreEqual(arg4, cmd1.Args[3]);
        }

        [TestMethod()]
        public void ParseTest3()
        {
            string arg1 = "arg1 \t arg1-2";
            string arg2 = "arg2";
            string arg3 = "arg3\r\narg3-1";
            string arg4 = "arg4 arg4-1 \0 \u9903 t";

            var cmd1 = InteractiveCommand.Parse($"run \"{arg1}\" {arg2} '{arg3}' `{arg4}`");
            Assert.AreEqual("run", cmd1.PrimaryCommand);
            Assert.AreEqual(4, cmd1.Args.Length);

            Assert.AreEqual(arg1, cmd1.Args[0]);
            Assert.AreEqual(arg2, cmd1.Args[1]);
            Assert.AreEqual(arg3, cmd1.Args[2]);
            Assert.AreEqual(arg4, cmd1.Args[3]);
        }

        [TestMethod()]
        public void ParseTest4()
        {
            string arg1 = "\nline1\nline2\nline3\n";

            var cmd1 = InteractiveCommand.Parse($"run `{arg1}`");
            Assert.AreEqual("run", cmd1.PrimaryCommand);
            Assert.AreEqual(1, cmd1.Args.Length);

            Assert.AreEqual(arg1, cmd1.Args[0]); 
        }

        [TestMethod()]
        public void ParseTestException()
        {
            Assert.ThrowsException<FormatException>(() =>
            {
                InteractiveCommand.Parse($"run sad\"");
            });

            Assert.ThrowsException<FormatException>(() =>
            {
                InteractiveCommand.Parse($"run sa\\td");
            });
        }
    }
}