using Microsoft.VisualStudio.TestTools.UnitTesting;
using MacroScript.Dsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroScript.Dsl.Tests
{
    [TestClass()]
    public class DslParserTests
    {
        [TestMethod()]
        public void ParseTest()
        {
            //var testText = """
            //    // 延迟 1 秒
            //    Delay(1000)

            //    // 在 (100, 100) 点击鼠标
            //    Mouse(LeftDown, 100, 100, 50)
            //    Mouse(LeftUp, 100, 100, 50)

            //    // 输入 Ctrl+C
            //    Key(KeyDown, LControlKey, 100)
            //    Key(KeyDown, C, 50)
            //    Key(KeyUp, C, 50)
            //    Key(KeyUp, LControlKey, 50)

            //    // 循环示例
            //    // Label(LoopStart)
            //    //     Delay(1000)
            //    //     if (PixelColor(0, 0) == RGB(255, 255, 255))
            //    //         Break // 如果左上角是白色则停止
            //    //     endif
            //    // Goto(LoopStart)

            //    // While 循环 (更简单)
            //    while (PixelColor(0, 0) != RGB(255, 255, 255))
            //        Delay(1000)
            //    endwhile
                
            //    """;

            //var collection = new DslParser().Parse(testText);

            //Assert.IsNotNull(collection);
        }

        [TestMethod()]
        public void NewParseTest()
        {
            var testText = """
                // 延迟 1 秒
                Delay(1000)

                // 在 (100, 100) 点击鼠标
                Mouse(LeftDown, 100, 100, 50)
                Mouse(LeftUp, 100, 100, 50)

                // 输入 Ctrl+C
                Key(KeyDown, LControlKey, 100)
                Key(KeyDown, C, 50)
                Key(KeyUp, C, 50)
                Key(KeyUp, LControlKey, 50)

                // 循环示例
                // Label(LoopStart)
                //     Delay(1000)
                //     if (PixelColor(0, 0) == RGB(255, 255, 255))
                //         Break // 如果左上角是白色则停止
                //     endif
                // Goto(LoopStart)

                // While 循环 (更简单)
                while (PixelColor(0, 0) != RGB(255, 255, 255))
                    Delay(1000)
                endwhile
                
                """;
            var lexer = new Lexer(testText);
            var collection = new NewDslParser().Parse(lexer.Tokenize());

            Assert.IsNotNull(collection);
        }
    }
}