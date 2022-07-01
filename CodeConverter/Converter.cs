using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeConverter
{
    class Converter
    {
        string[] removes = { "public","private","protected" ,"\r\n"};
        string[] blockparts = { @"namespace +\w+ *\{.+\}"
        , @"class .*\{.+\}"
        , @"secretclass00 *\( *(\w+ +\w+,?)* *\) *\{.*\}"
        , @"\w+ +\w+ *\( *(\w+ +\w+,?)* *\) *\{ *.*\ *\}"
        , @"else *.*\{ *.*\ *}"
        , @"if *\( *((\w+|\(|\)|\=+|\<|\>)+( +or|and)?)* *\) *\{ *.*\ *}"
        };//(if? *\( *(.+)* *\))*
        public string pycode = "";
        string[] classes = { @"int +(\w|_)+ *(=|\+=|-=)", @"double +(\w|_)+ *(=|\+=|-=)", @"string +(\w|_)+ *(=|\+=|-=)", @"(\w|_)+ *(=|\+=|-=)" };
        string[] removesw = {"int","double","string","" };
        string[] quickcheck = { @"if( |\()+", @"else( |\{)+","class +", "namespace +", @"\w+ +\w+ *\( *(\w+ +\w+,?)* *\)", @"\w+ *\( *(\w+ +\w+,?)* *\) *\{" };
        string Code;
        string space = "";
        enum types{bull,namespacecover, classcover, initcover, functioncover, ifcover, elsecover,inside };
        List<string> variables;
        List<Converter> cons;
        types cover;
        string motherclass2;
        List<catchsolve> catchs;
        //string motherfunc2;
        public  class catchsolve
        {
            public string rege = @"int +(\w|_)+";
            public string remove = "int";
            int removi = 0;
            public catchsolve(string rg,string rm)
            {
                rege = rg;remove = rm;
            }
            public bool check(string line)
            {
                Regex rg = new Regex(rege);
                return (rg.Match(line).Value != "");
            }
            public string getback(string line)
            {
                // string r = "";
                var line2 = line;
                Regex rg = new Regex(rege);
                var va = rg.Match(line).Value;
                line = line.Replace(va, "");
                Converter cv = new Converter(line);
                cv.cover = types.inside;
                cv.convert();
                
                if (remove != "")
                {
                    rg = new Regex(remove);
                    removi = rg.Match(line2).Index;
                    var s = rg.Match(line2).Value;
                    va = va.Substring(s.Length + removi).Trim(' ');
                }
                va = va+cv.pycode;
                return va;
            }


        }
        public Converter(string code,string motherclass="secretclass00",string motherfunc= "moterfuncsecret")
        {
            foreach(var i in removes)
            {
                code = code.Replace(i, "");
            }
            code = code.Replace("||", " or ");
            code = code.Replace("&&", " and ");
            Code = code;
            variables = new List<string>();
            cons = new List<Converter>();
            cover = types.bull;
            blockparts[2]= motherclass+@" *\( *(\w+ +\w+,?)* *\) *\{.*\}";
       //     blockparts[4]= motherfunc+ @" *\w+ *\( *(\w+ +\w+,?)* *\) *\{ *.*\ *}";
            motherclass2 = motherclass;
            catchs = new List<catchsolve>();
            int i2 = 0;
            foreach(var k in classes)
            {
                catchsolve ct = new catchsolve(k,removesw[i2]);
                catchs.Add(ct);
                i2++;
            }
        //    motherfunc2 = motherfunc;
        }
        string morespace = "  ";
        public void convert()
        {
            Regex rg;
            var sf = "";
            var sf2 = "";
            Char[] c = { '{', '}' };
            Char[] c2 = {  ' ' };
            Char[] c3 = {  '(',')' };
            string qh;
            string add;
            add = "";
            if (motherclass2 != "secretclass00")
            {
                add = "self";
            }
            switch (cover)
            {
                case types.namespacecover:
                    rg = new Regex(@"namespace +\w+");
                     sf = rg.Match(Code).Value;
                    Code = Code.Replace(sf, "");
                    Code = Code.Trim(c2);
                    Code = Code.Trim(c);
                    break;
                case types.classcover:
                    rg = new Regex(@"class +\w+");
                    sf= rg.Match(Code).Value;
                    pycode += space + sf + ":\r\n";
                    space +=morespace;
                    Code = Code.Replace(sf, "");
                    Code = Code.Trim(c2);
                    Code = Code.Trim(c);
                    break;
                case types.functioncover:
                    rg = new Regex(@"\w+ +\w+ *\( *(\w+ +\w+,?)* *\)");
                    sf = rg.Match(Code).Value;
                    rg = new Regex(@"\( *(\w+ +\w+,?)* *\)");
                    qh = rg.Match(Code).Value.Trim(c3);
                    Converter cv = new Converter(qh);
                    cv.cover = types.inside;
                    cv.convert();
                    rg = new Regex(@"\w+ +");
                    qh = Code.Replace(rg.Match(Code).Value, "");
                    rg = new Regex(@"\w+ *");
                    qh = rg.Match(qh).Value;
                    if (add != "" && cv.pycode != "")
                    {
                        add += ",";
                    }
                    pycode += space + "def " +qh +"("+add+cv.pycode+"):\r\n";
                    space += morespace;
                    Code = Code.Replace(sf, "");
                    Code = Code.Trim(c2);
                    Code = Code.Trim(c);
                    break;
                case types.initcover:
                    rg = new Regex(motherclass2+@" *\( *(\w+ +\w+,?)* *\)");
                    sf = rg.Match(Code).Value;
                    rg = new Regex(@"\( *(\w+ +\w+\,?)* *\)");
                    qh = rg.Match(Code).Value;
                    qh = qh.Trim(c3);
                    Converter cv2 = new Converter(qh);
                    cv2.cover = types.inside;
                    cv2.convert();
                    rg = new Regex(@"\w +");
                    if (add!="" && cv2.pycode != "")
                    {
                        add += ",";
                    }
                    pycode += space+ "def init("+add+cv2.pycode+"):\r\n";
                    space += morespace;
                    Code = Code.Replace(sf, "");
                    Code = Code.Trim(c2);
                    Code = Code.Trim(c);
                    break;
                case types.ifcover:
                    rg = new Regex(@"if *\( *(.+)* *\) *");
                    sf= rg.Match(Code).Value;
                    rg = new Regex(@"\( *((\w+|\(|\)|\=+|\<|\>)+( +or|and)?)* *\)");
                    qh = rg.Match(Code).Value.Trim(c3);
                    Converter cv3 = new Converter(qh);
                    cv3.cover = types.inside;
                    cv3.convert();
                    pycode += space + "if " + cv3.pycode + " :\r\n";
                    space += morespace;
                    Code = Code.Replace(sf, "");
                    Code = Code.Trim(c2);
                    Code = Code.Trim(c);
                    break;
                case types.elsecover:
                    rg = new Regex("else");
                    sf = rg.Match(Code).Value;
                    rg = new Regex(@" .*");
                    qh = rg.Match(Code).Value;
                    qh = qh.Trim(c2);
                    if (qh.Substring(0,2) != "if")
                    {
                        qh = qh.Trim(c);
                    }
                    Converter cv4 = new Converter(qh);
                    cv4.cover = types.bull;
                    cv4.space = space + morespace;
                    cv4.convert();
                    pycode += space + "else : \r\n" + cv4.pycode + "\r\n";
                    space += morespace;
                    Code = Code.Replace(sf, "");
                    Code = Code.Trim(c2);
                    Code = Code.Trim(c);
                    return;

            }
            if (Code != "")
            {
                codeconverter();
            }
            blockmodifier();
          //  pycode = "";
            
            foreach (var cc in sortindx(cons))
            {
                done = false;
                cc.convert();
                pycode += cc.pycode;
            }
            if (Code != "")
            {
                codeconverter();
            }
        }
        bool done;
        void codeconverter()
        {
            if (done)
            { return; }
            string[] lines;
            if (cover == types.inside)
            {
                 lines = Code.Split(',');
            }
            else
            {
                 lines = Code.Split(';');
            }
            bool breakme = false;
            int l = 0;
            for (int i= 0;i< lines.Length; i++)
            {
               foreach(var k in quickcheck)
                {
                    Regex rg = new Regex(k);
                    var s = rg.Match(lines[i]).Value;
                    if (s != "")
                    {
                        breakme = true;
                        break;
                    }
                }
               if (breakme)
                {
                    break;
                }

               foreach(var d in catchs)
                {
                    if (d.check(lines[i]))
                    {
                        pycode +=space+ d.getback(lines[i]);
                        if (cover != types.inside)
                        {
                            pycode += "\r\n";
                        }
                        if (cover == types.inside)
                        {
                            Code = Code.Remove(l, lines[i].Length);
                        }
                        else
                        {
                            Code = Code.Remove(l, lines[i].Length + 1);
                        }
                        l -= lines[i].Length;
                        break;
                    }

                    //if (gf != "")
                    //{
                    //    done = true;
                    //    Regex rn = new Regex(@"\w+");
                    //    var d3 = rn.Match(d).Value;
                    //    string re = gf.Replace(d3, "").Trim(' ');
                    //    var k = re.Split('=');
                    //    pycode += space + k[0];

                    //    if (k.Length > 1)
                    //    {
                    //        pycode+= " = ";
                    //        var sff = k[1];
                    //        Converter cv = new Converter(sff);
                    //        cv.convert();
                    //        pycode +=cv.pycode;
                    //    }
                    //    if (cover != types.inside)
                    //    {
                    //        pycode += "\r\n";
                    //    }
                    //    if (cover == types.inside)
                    //    {
                    //        Code = Code.Remove(l, lines[i].Length);
                    //    }
                    //    else
                    //    {
                    //        Code = Code.Remove(l, lines[i].Length+1);
                    //    }
                    //    l -= lines[i].Length;
                    //    break;
                    //}
                }
               if (pycode == "")
                {
                    pycode += Code;
                    Code = "";
                }
                l += lines[i].Length;
            }
        }
        List<int> ks;
        int myindx;
        static IEnumerable<Converter> sortindx(IEnumerable<Converter> e)
        {
            // Use LINQ to sort the array received and return a copy.
            var sorted = from s in e
                         orderby s.myindx ascending
                         select s;
            return sorted;
        }
        void blockmodifier()
        {
           int i = 0;
            Code = Code.Trim(' ');
            foreach (var s in blockparts)
            {
                if (Code == "")
                {
                    break;
                }
            here:
                Regex rg = new Regex(s);
                string st = rg.Match(Code).Value;
                var k = rg.Match(Code).Index;
               // ks.Add(k);
                if (st != "")
                {
                    
                    Converter cv = new Converter(Code,motherclass2);
                    cv.space = space;
                    if (i == 1)
                    {
                        var st2 = (new Regex(@"class +\w+")).Match(st).Value;
                        st2 = st2.Replace("class", "");
                        st2 = st2.Replace(" ", "");
                        cv = new Converter(st, st2);
                        List<string> g = new List<string>();
                        foreach(var ch in classes)
                        {
                            g.Add(ch);
                        }
                        g.Add(st2);
                        classes = new string[g.Count];
                        for(int ii = 0; ii < classes.Length; ii++)
                        {
                            classes[ii] = g[ii];
                        }
                        st= (new Regex(@"class +\w+ *")).Match(st).Value + getened(st, "{", "}");
                    }
                    else if (i == 3 || i == 2||i==4||i==5)
                    {
                        //var st2 = (new Regex(@"\w+ +\w+")).Match(st).Value;
                        //var st3 = (new Regex(@"\w+")).Match(st).Value;
                        //st2 = st2.Replace(st3, "");
                        //st2 = st2.Replace(" ", "");
                        //cv = new Converter(st,motherfunc: st2);
                        string part1 = "";
                        if (i == 3)
                        {
                            Regex rg2 = new Regex(@"\w+ *\w+ *\( *(\w+ +\w+,?)* *\) *");
                            part1 = rg2.Match(Code).Value;
                        }
                        else if (i==2)
                        {
                            Regex rg2 = new Regex(motherclass2 + @" *\( *(\w+ +\w+,?)* *\) *");
                            part1 = rg2.Match(Code).Value;
                        }
                        else if (i == 5)
                        {
                            Regex rg2 = new Regex(@"if *\( *((\w+|\(|\)|\=+|\<|\>)+( +or|and)?)* *\)");
                            part1 = rg2.Match(Code).Value;
                        }
                        else if (i == 4)
                        {
                            Regex rg2 = new Regex(@"else *if *\( *((\w+|\(|\)|\=+|\<|\>)+( +or|and)?)* *\)");
                            part1 = rg2.Match(Code).Value;
                         //   part1 = "else ";
                        }
                        st = part1+ getened(st, "{", "}");
                    }
                    cv.Code = st;
                    cv.cover = gettyped(i);
                    cv.myindx = k;
                    cons.Add(cv);
                    Code = Code.Replace(st, "");
                    if (rg.Match(Code).Value != "")
                    {
                        goto here;
                    }
                }
                i++;
            }
        }
        types gettyped(int indx)
        {
            types g = types.classcover;
            switch (indx)
            {
                case 0:
                    g = types.namespacecover;
                    break;
                case 1:
                    g = types.classcover;
                    break;
                case 3:
                    g = types.functioncover;
                    break;
                case 2:
                    g = types.initcover;
                    break;
                case 5:
                    g = types.ifcover;
                    break;
                case 4:
                    g = types.elsecover;
                    break;

            }
            return g;
        }
        string getened(string txt,string paranc1,string paranc2)
        {
            string ans = "";
            int numparance = -1;
            int start = 0;
            int end = 0;
            for(int i = 0; i < txt.Length; i++)
            {
                string q = txt.Substring(i, 1);
                if (numparance == -1)
                {
                    if (q == paranc1)
                    {
                        numparance = 0;
                        start = i;
                    }
                }
                if (q == paranc1)
                {
                    numparance++;
                }
                else if (q == paranc2)
                {
                    numparance--;
                }
                if (numparance == 0)
                {
                    end = i-start+1;
                    break;
                }
            }
            ans = txt.Substring(start, end);
            return ans;
        }
    }
}
