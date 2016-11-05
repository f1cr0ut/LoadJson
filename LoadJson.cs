/*
・LoadJson able to parse json format[RFC7159] string.
・LoadJson able to parse c-style comment.
・LoadJson don't check whether json is well-formed or not. Be careful.
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

public class LoadJson {

    private StringReader JsonChar;

    // raw_json must be utf8
    public object Load(string raw_json) {
        using(JsonChar = new StringReader(raw_json)) {
            while(true) {
                switch(JsonChar.Read()) {
                    case '{':
                        return ParseObjects();
                    case '[':
                        return ParseArrays();
                    case -1:
                        return null;
                    case '/':
                        SkipComments();
                        break;
                }
            }
        }
        return null;
    }

    private List<object> ParseArrays() {
        List<object> arr = new List<object>();
        bool loop = true;
        while(loop) {
            var character = JsonChar.Peek();
            switch(character) {
                case '/':
                    JsonChar.Read();
                    SkipComments();
                    break;
                case ']':
                    loop = false;
                    JsonChar.Read();
                    break;
                default:
                    var value = ParseValues();
                    if(value != null) {
                        arr.Add(value);
                    }
                    break;
            }
        }
        return arr;
    }

    private Dictionary<string, object> ParseObjects() {
        Dictionary<string, object> dic = new Dictionary<string, object>();
        bool loop = true;
        while(loop) {
            var character = JsonChar.Read();
            switch(character) {
                case '/':
                    SkipComments();
                    break;
                case '}':
                    loop = false;
                    break;
                case '\"':
                    string name = ParseString();
                    while(JsonChar.Read() != ':'){}
                    dic[name] = ParseValues();
                    break;
            }
        }
        return dic;
    }

    private string ParseString() {
        StringBuilder result_str = new StringBuilder();
        bool loop = true;
        while(loop) {
            var character = JsonChar.Read();
            switch(character) {
                case '\"':
                    loop = false;
                    break;
                case '\\':
                    ParseSpecialCharacters(result_str);
                    break;
                default:
                    result_str.Append(Convert.ToChar(character));
                    break;
            }
        }
        return result_str.ToString();
    }
    
    private void ParseSpecialCharacters(StringBuilder result_str) {
        switch(JsonChar.Peek()) {
            // convert hex numbers to the unicode character
            case 'u':
                JsonChar.Read();
                var hex_numbers = new char[4];
                for (int i=0; i< 4; i++) {
                     hex_numbers[i] = Convert.ToChar(JsonChar.Read());
                }
                result_str.Append((char)Convert.ToInt32(new string(hex_numbers), 16));
                break;
            // do nothing for others
            default:
                result_str.Append('\\');
                result_str.Append(Convert.ToChar(JsonChar.Read()));
                break;
        }
    }

    private object ParseValues() {
        bool loop = true;
        while(loop) {
            var character = JsonChar.Peek();
            switch(character) {
                case '/':
                    JsonChar.Read();
                    SkipComments();
                    break;
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return ParseNumbers();
                case ']':
                case '}':
                    loop = false;
                    break;
                case '[':
                    JsonChar.Read();
                    return ParseArrays();
                case '{':
                    JsonChar.Read();
                    return ParseObjects();
                case '\"':
                    JsonChar.Read();
                    return ParseString();
                // only small characters    
                case 't':
                case 'f':
                case 'n':
                    StringBuilder str_builder = new StringBuilder();
                    for(int i = 0; i < 4; ++i) {
                        str_builder.Append(Convert.ToChar(JsonChar.Read()));
                    }
                    string str_value = str_builder.ToString();
                    if(str_value == "true") return true;
                    else if(str_value == "null") return null;
                    else if(str_value == "fals" && JsonChar.Read() == 'e') return false;
                    else throw new Exception();
                default:
                    JsonChar.Read();
                    break;
            }
        }
        // return null if value is empty
        return null;
    }

    private object ParseNumbers() {
        StringBuilder str_num = new StringBuilder();
        bool loop = true;
        while(loop) {
            var character = JsonChar.Peek();
            switch(character) {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '.':
                case 'e':
                case 'E':
                case '+':
                case '-':
                    str_num.Append(Convert.ToChar(JsonChar.Read()));
                    break;
                default:
                    loop = false;
                    break;
            }
        }
        if(str_num.ToString().IndexOf('.') == -1) {
            return Convert.ToInt64(str_num.ToString(), 10);
        }
        return Convert.ToDouble(str_num.ToString());
    }

    private void SkipComments() {
        if(JsonChar.Peek() == '*') {
            JsonChar.Read();
            while(JsonChar.Read() != '*') {
                if(JsonChar.Peek() == '/') {
                    JsonChar.Read();
                    break;
                }
            }
        }
    }
}
