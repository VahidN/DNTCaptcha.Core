using System.Collections.Generic;
using System.Linq;

namespace DNTCaptcha.Core;

/// <summary>
///     Convert a number into words
/// </summary>
public class HumanReadableIntegerProvider : ICaptchaTextProvider
{
    private readonly Dictionary<Language, string> _and = new()
    {
        {
            Language.English, " "
        },
        {
            Language.Persian, " و "
        },
        {
            Language.Norwegian, " og "
        },
        {
            Language.Italian, " "
        },
        {
            Language.Turkish, " "
        },
        {
            Language.Arabic, " و "
        },
        {
            Language.Russian, " "
        },
        {
            Language.Chinese, " "
        },
        {
            Language.Spanish, " "
        },
        {
            Language.Portuguese, " "
        },
        {
            Language.French, " et "
        },
        {
            Language.German, " und "
        },
        {
            Language.Dutch, " en "
        },
        {
            Language.Azerbaijani, " "
        }
    };

    private readonly Dictionary<Language, string> _negative = new()
    {
        {
            Language.English, "Negative "
        },
        {
            Language.Persian, "منهای "
        },
        {
            Language.Norwegian, "Negativ"
        },
        {
            Language.Italian, "Negativo"
        },
        {
            Language.Turkish, "Eksi"
        },
        {
            Language.Arabic, "سالب "
        },
        {
            Language.Russian, "Минус "
        },
        {
            Language.Chinese, "负"
        },
        {
            Language.Spanish, "Negativo"
        },
        {
            Language.Portuguese, "Negativo"
        },
        {
            Language.French, "Négatif"
        },
        {
            Language.German, "Negativ"
        },
        {
            Language.Dutch, "Negatief"
        },
        {
            Language.Azerbaijani, "Neqativ"
        }
    };

    private readonly IList<NumberWord> _numberWords =
    [
        new()
        {
            Group = DigitGroup.Ones,
            Language = Language.English,
            Names = (List<string>)
                [
                    string.Empty, "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine"
                ]
        },
        new()
        {
            Group = DigitGroup.Ones,
            Language = Language.Persian,
            Names = (List<string>) [string.Empty, "یک", "دو", "سه", "چهار", "پنج", "شش", "هفت", "هشت", "نه"]
        },
        new()
        {
            Group = DigitGroup.Ones,
            Language = Language.Norwegian,
            Names = (List<string>) [string.Empty, "en", "to", "tre", "fire", "fem", "seks", "syv", "åtte", "ni"]
        },
        new()
        {
            Group = DigitGroup.Ones,
            Language = Language.Italian,
            Names = (List<string>)
            [
                string.Empty, "Uno", "Due", "Tre", "Quattro", "Cinque", "Sei", "Sette", "Otto", "Nove"
            ]
        },
        new()
        {
            Group = DigitGroup.Ones,
            Language = Language.Turkish,
            Names = (List<string>) [string.Empty, "Bir", "İki", "Üç", "Dört", "Beş", "Altı", "Yedi", "Sekiz", "Dokuz"]
        },
        new()
        {
            Group = DigitGroup.Ones,
            Language = Language.Arabic,
            Names = (List<string>)
            [
                string.Empty, "واحد", "اثنان", "ثلاثة", "اربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة"
            ]
        },
        new()
        {
            Group = DigitGroup.Ones,
            Language = Language.Russian,
            Names = (List<string>)
            [
                string.Empty, "Один", "Два", "Три", "Четыре", "Пять", "Шесть", "Семь", "Восемь", "Девять"
            ]
        },
        new()
        {
            Group = DigitGroup.Ones,
            Language = Language.Chinese,
            Names = (List<string>) [string.Empty, "一", "二", "三", "四", "五", "六", "七", "八", "九"]
        },
        new()
        {
            Group = DigitGroup.Ones,
            Language = Language.Spanish,
            Names = (List<string>)
            [
                string.Empty, "Uno", "Dos", "Tres", "Cuatro", "Cinco", "Seis", "Siete", "Ocho", "Nueve"
            ]
        },
        new()
        {
            Group = DigitGroup.Ones,
            Language = Language.Portuguese,
            Names = (List<string>)
            [
                string.Empty, "Um", "Dois", "Três", "Quatro", "Cinco", "Seis", "Sete", "Oito", "Nove"
            ]
        },
        new()
        {
            Group = DigitGroup.Ones,
            Language = Language.French,
            Names = (List<string>)
                [
                    string.Empty, "Un", "Deux", "Trois", "Quatre", "Cinq", "Six", "Sept", "Huit", "Neuf"
                ]
        },
        new()
        {
            Group = DigitGroup.Ones,
            Language = Language.German,
            Names = (List<string>)
            [
                string.Empty, "Eins", "Zwei", "Drei", "Vier", "Fünf", "Sechs", "Sieben", "Acht", "Neun"
            ]
        },
        new()
        {
            Group = DigitGroup.Ones,
            Language = Language.Dutch,
            Names = (List<string>)
                [
                    string.Empty, "Een", "Twee", "Drie", "Vier", "Vijf", "Zes", "Zeven", "Acht", "Negen"
                ]
        },
        new()
        {
            Group = DigitGroup.Ones,
            Language = Language.Azerbaijani,
            Names = (List<string>)
                [
                    string.Empty, "Bir", "İki", "Üç", "Dörd", "Beş", "Altı", "Yeddi", "Səkkiz", "Doqquz"
                ]
        },
        new()
        {
            Group = DigitGroup.Teens,
            Language = Language.English,
            Names = (List<string>)
            [
                "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen",
                "Nineteen"
            ]
        },
        new()
        {
            Group = DigitGroup.Teens,
            Language = Language.Persian,
            Names = (List<string>)
            [
                "ده", "یازده", "دوازده", "سیزده", "چهارده", "پانزده", "شانزده", "هفده", "هجده", "نوزده"
            ]
        },
        new()
        {
            Group = DigitGroup.Teens,
            Language = Language.Norwegian,
            Names = (List<string>)
            [
                "ti", "elleve", "tolv", "tretten", "fjorten", "femten", "seksten", "sytten", "atten", "nitten"
            ]
        },
        new()
        {
            Group = DigitGroup.Teens,
            Language = Language.Italian,
            Names = (List<string>)
            [
                "Dieci", "Undici", "Dodici", "Tredici", "Quattordici", "Quindici", "Sedici", "Diciassette",
                "Diciotto", "Diciannove"
            ]
        },
        new()
        {
            Group = DigitGroup.Teens,
            Language = Language.Turkish,
            Names = (List<string>)
            [
                "On", "Onbir", "Oniki", "Onüç", "Ondört", "Onbeş", "Onaltı", "Onyedi", "Onsekiz", "Ondokuz"
            ]
        },
        new()
        {
            Group = DigitGroup.Teens,
            Language = Language.Arabic,
            Names = (List<string>)
            [
                "عشرة", "احدى عشر", "اثني عشر", "ثلاثة عشر", "اربعة عشر", "خمسة عشر", "ستة عشر", "سبعة عشر",
                "ثمانية عشر", "تسعة عشر"
            ]
        },
        new()
        {
            Group = DigitGroup.Teens,
            Language = Language.Russian,
            Names = (List<string>)
            [
                "Десять", "Одинадцать", "Двенадцать", "Тринадцать", "Четырнадцать", "Пятнадцать", "Шестнадцать",
                "Семнадцать", "Восемнадцать", "Девятнадцать"
            ]
        },
        new()
        {
            Group = DigitGroup.Teens,
            Language = Language.Chinese,
            Names = (List<string>) ["十", "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九"]
        },
        new()
        {
            Group = DigitGroup.Teens,
            Language = Language.Spanish,
            Names = (List<string>)
            [
                string.Empty, "Diez", "Once", "Doce", "Trece", "Catorce", "Quince", "Dieciséis", "Diecisiete",
                "Dieciocho", "Diecineve"
            ]
        },
        new()
        {
            Group = DigitGroup.Teens,
            Language = Language.Portuguese,
            Names = (List<string>)
            [
                "Dez", "Onze", "Doze", "Treze", "Catorze", "Quinze", "Dezaseis", "Dezasete", "Dezoito", "Dezanove"
            ]
        },
        new()
        {
            Group = DigitGroup.Teens,
            Language = Language.French,
            Names = (List<string>)
            [
                "Dix", "Onze", "Douze", "Treize", "Quatorze", "Quinze", "Seize", "Dix-sept", "Dix-huit", "Dix-neuf"
            ]
        },
        new()
        {
            Group = DigitGroup.Teens,
            Language = Language.German,
            Names = (List<string>)
            [
                "Zehn", "Elf", "Zwölf", "Dreizehn", "Vierzehn", "Fünfzehn", "Sechzehn", "Siebzehn", "Achtzehn",
                "Neunzehn"
            ]
        },
        new()
        {
            Group = DigitGroup.Teens,
            Language = Language.Dutch,
            Names = (List<string>)
            [
                "Tien", "Elf", "Twaalf", "Dertien", "Veertien", "Vijftien", "Zestien", "Zeventien", "Achttien",
                "Negentien"
            ]
        },
        new()
        {
            Group = DigitGroup.Teens,
            Language = Language.Azerbaijani,
            Names = (List<string>)
            [
                "On", "Onbir", "Oniki", "Onüç", "Ondörd", "Onbeş", "Onaltı", "Onyeddi", "Onsəkkiz", "Ondoqquz"
            ]
        },
        new()
        {
            Group = DigitGroup.Tens,
            Language = Language.English,
            Names = (List<string>) ["Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"]
        },
        new()
        {
            Group = DigitGroup.Tens,
            Language = Language.Persian,
            Names = (List<string>) ["بیست", "سی", "چهل", "پنجاه", "شصت", "هفتاد", "هشتاد", "نود"]
        },
        new()
        {
            Group = DigitGroup.Tens,
            Language = Language.Norwegian,
            Names = (List<string>) ["tjue", "tretti", "førti", "femti", "seksti", "sytti", "åtti", "nitti"]
        },
        new()
        {
            Group = DigitGroup.Tens,
            Language = Language.Italian,
            Names = (List<string>)
            [
                "Venti", "Trenta", "Quaranta", "Cinquanta", "Sessanta", "Settanta", "Ottanta", "Novanta"
            ]
        },
        new()
        {
            Group = DigitGroup.Tens,
            Language = Language.Turkish,
            Names = (List<string>) ["Yirmi", "Otuz", "Kırk", "Elli", "Altmış", "Yetmiş", "Seksen", "Doksan"]
        },
        new()
        {
            Group = DigitGroup.Tens,
            Language = Language.Arabic,
            Names = (List<string>) ["عشرون", "ثلاثون", "اربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون"]
        },
        new()
        {
            Group = DigitGroup.Tens,
            Language = Language.Russian,
            Names = (List<string>)
            [
                "Двадцать", "Тридцать", "Сорок", "Пятьдесят", "Шестьдесят", "Семьдесят", "Восемьдесят", "Девяносто"
            ]
        },
        new()
        {
            Group = DigitGroup.Tens,
            Language = Language.Chinese,
            Names = (List<string>) ["二十", "三十", "四十", "五十", "六十", "七十", "八十", "九十"]
        },
        new()
        {
            Group = DigitGroup.Tens,
            Language = Language.Spanish,
            Names = (List<string>)
            [
                "Veinte", "Treinta", "Cuarenta", "Cincuenta", "Sesenta", "Setenta", "Ochenta", "Noventa"
            ]
        },
        new()
        {
            Group = DigitGroup.Tens,
            Language = Language.Portuguese,
            Names = (List<string>)
            [
                "Vinte", "Trinta", "Quarnta", "Cinquenta", "Sessenta", "Setenta", "Oitenta", "Noventa"
            ]
        },
        new()
        {
            Group = DigitGroup.Tens,
            Language = Language.French,
            Names = (List<string>)
            [
                "Vingt", "Trente", "Quarante", "Cinquante", "Soixante", "Soixante-dix", "Quatre-vingts",
                "Quatre-vingt-dix"
            ]
        },
        new()
        {
            Group = DigitGroup.Tens,
            Language = Language.German,
            Names = (List<string>)
            [
                "Zwanzig", "Dreißig", "Vierzig", "Fünfzig", "Sechzig", "Siebzig", "Achtzig", "Neunzig"
            ]
        },
        new()
        {
            Group = DigitGroup.Tens,
            Language = Language.Dutch,
            Names = (List<string>)
            [
                "Twintig", "Dertig", "Veertig", "Vijftig", "Zestig", "Zeventig", "Tachtig", "Negentig"
            ]
        },
        new()
        {
            Group = DigitGroup.Tens,
            Language = Language.Azerbaijani,
            Names = (List<string>) ["İyirmi", "Otuz", "Qırx", "Əlli", "Altmış", "Yetmiş", "Səksən", "Doxsan"]
        },
        new()
        {
            Group = DigitGroup.Hundreds,
            Language = Language.English,
            Names = (List<string>)
            [
                string.Empty, "One Hundred", "Two Hundred", "Three Hundred", "Four Hundred", "Five Hundred",
                "Six Hundred", "Seven Hundred", "Eight Hundred", "Nine Hundred"
            ]
        },
        new()
        {
            Group = DigitGroup.Hundreds,
            Language = Language.Persian,
            Names = (List<string>)
            [
                string.Empty, "یکصد", "دویست", "سیصد", "چهارصد", "پانصد", "ششصد", "هفتصد", "هشتصد", "نهصد"
            ]
        },
        new()
        {
            Group = DigitGroup.Hundreds,
            Language = Language.Norwegian,
            Names = (List<string>)
            [
                string.Empty, "ett hundre", "to hundre", "tre hundre", "fire hundre", "fem hundre", "seks hundre",
                "syv hundre", "åtte hundre", "ni hundre"
            ]
        },
        new()
        {
            Group = DigitGroup.Hundreds,
            Language = Language.Italian,
            Names = (List<string>)
            [
                string.Empty, "Cento", "Duecento", "Trecento", "Quattrocento", "Cinquecento", "Seicento",
                "Settecento", "Ottocento", "Novecento"
            ]
        },
        new()
        {
            Group = DigitGroup.Hundreds,
            Language = Language.Turkish,
            Names = (List<string>)
            [
                string.Empty, "Yüz", "İki Yüz", "Üç Yüz", "Dört Yüz", "Beş Yüz", "Altı Yüz", "Yedi Yüz",
                "Sekiz Yüz", "Dokuz Yüz"
            ]
        },
        new()
        {
            Group = DigitGroup.Hundreds,
            Language = Language.Arabic,
            Names = (List<string>)
            [
                string.Empty, "مائة", "مائتان", "ثلاثمائة", "اربعمائة", "خمسمائة", "ستمائة", "سبعمائة", "ثمانمائة",
                "تسعمائة"
            ]
        },
        new()
        {
            Group = DigitGroup.Hundreds,
            Language = Language.Russian,
            Names = (List<string>)
            [
                string.Empty, "Сто", "Двести", "Триста", "Четыреста", "Пятьсот", "Шестьсот", "Семьсот", "Восемьсот",
                "Девятьсот"
            ]
        },
        new()
        {
            Group = DigitGroup.Hundreds,
            Language = Language.Chinese,
            Names = (List<string>) [string.Empty, "一百", "两百", "三百", "四百", "五百", "六百", "七百", "八百", "九百"]
        },
        new()
        {
            Group = DigitGroup.Hundreds,
            Language = Language.Spanish,
            Names = (List<string>)
            [
                string.Empty, "Cien", "Doscientos", "Trescientos", "Cuatrocientos", "Quinientos", "Seiscientos",
                "Setecientos", "Ochocientos", "Novecientos"
            ]
        },
        new()
        {
            Group = DigitGroup.Hundreds,
            Language = Language.Portuguese,
            Names = (List<string>)
            [
                string.Empty, "Cem", "Duzentos", "Trezentos", "Quatrocentos", "Quinhentos", "Seiscentos",
                "Setecentos", "Oitocentos", "Novecentos"
            ]
        },
        new()
        {
            Group = DigitGroup.Hundreds,
            Language = Language.French,
            Names = (List<string>)
            [
                string.Empty, "Cent", "Deux Cents", "Trois Cents", "Quatre Cents", "Cinq Cents", "Six Cents",
                "Sept Cents", "Huit Cents", "Neuf Cents"
            ]
        },
        new()
        {
            Group = DigitGroup.Hundreds,
            Language = Language.German,
            Names = (List<string>)
            [
                string.Empty, "Einhundert", "Zweihundert", "Dreihundert", "Vierhundert", "Fünfhundert",
                "Sechshundert", "Siebenhundert", "Achthundert", "Neunhundert"
            ]
        },
        new()
        {
            Group = DigitGroup.Hundreds,
            Language = Language.Dutch,
            Names = (List<string>)
            [
                string.Empty, "Honderd", "Tweehonderd", "Driehonderd", "Vierhonderd", "Vijfhonderd", "Zeshonderd",
                "Zevenhonderd", "Achthonderd", "Negenhonderd"
            ]
        },
        new()
        {
            Group = DigitGroup.Hundreds,
            Language = Language.Azerbaijani,
            Names = (List<string>)
            [
                string.Empty, "Yüz", "İki yüz", "Üç yüz", "Dörd yüz", "Beş yüz", "Altı yüz", "Yeddi yüz",
                "Səkkiz yüz", "Doqquz yüz"
            ]
        },
        new()
        {
            Group = DigitGroup.Thousands,
            Language = Language.English,
            Names = (List<string>)
            [
                string.Empty, " Thousand", " Million", " Billion", " Trillion", " Quadrillion", " Quintillion",
                " Sextillian", " Septillion", " Octillion", " Nonillion", " Decillion", " Undecillion",
                " Duodecillion", " Tredecillion", " Quattuordecillion", " Quindecillion", " Sexdecillion",
                " Septendecillion", " Octodecillion", " Novemdecillion", " Vigintillion", " Unvigintillion",
                " Duovigintillion", " 10^72", " 10^75", " 10^78", " 10^81", " 10^84", " 10^87", " Vigintinonillion",
                " 10^93", " 10^96", " Duotrigintillion", " Trestrigintillion"
            ]
        },
        new()
        {
            Group = DigitGroup.Thousands,
            Language = Language.Persian,
            Names = (List<string>)
            [
                string.Empty, " هزار", " میلیون", " میلیارد", " تریلیون", " Quadrillion", " Quintillion",
                " Sextillian", " Septillion", " Octillion", " Nonillion", " Decillion", " Undecillion",
                " Duodecillion", " Tredecillion", " Quattuordecillion", " Quindecillion", " Sexdecillion",
                " Septendecillion", " Octodecillion", " Novemdecillion", " Vigintillion", " Unvigintillion",
                " Duovigintillion", " 10^72", " 10^75", " 10^78", " 10^81", " 10^84", " 10^87", " Vigintinonillion",
                " 10^93", " 10^96", " Duotrigintillion", " Trestrigintillion"
            ]
        },
        new()
        {
            Group = DigitGroup.Thousands,
            Language = Language.Norwegian,
            Names = (List<string>)
            [
                string.Empty, " tusen", " million", " milliard", " billion", " billiard", " trillion", " trilliard",
                " kvadrillion", " kvintillion", " sekstillion", " septillion", " oktillion", " nonillion", " desillion",

                // Not translated the next
                " Quattuordecillion", " Quindecillion", " Sexdecillion", " Septendecillion", " Octodecillion",
                " Novemdecillion", " Vigintillion", " Unvigintillion", " Duovigintillion", " 10^72", " 10^75", " 10^78",
                " 10^81", " 10^84", " 10^87", " Vigintinonillion", " 10^93", " 10^96", " Duotrigintillion",
                " Trestrigintillion"
            ]
        },
        new()
        {
            Group = DigitGroup.Thousands,
            Language = Language.Italian,
            Names = (List<string>)
            [
                string.Empty, "mila", "Milioni", "Miliardi", "Bilioni", "Biliardi", "Trilioni", "Triliardi",
                " Quadrilioni", "Quadriliardi", "Quintilioni", "Quintiliardi", "Sistilioni", "Sistiliardi",
                "Settilioni", " Settiliardi", " Ottilioni", "Ottiliardi", "Novilioni", "Noviliardi", "Decilioni",
                " Deciliardi", "Undicilioni", "Undiciliardi ", "Dodicilioni", "Dodiciliardi", "Tredicilioni",
                "Trediciliardi", "Quattordicilioni", "Quattordiciliardi", "Quindicilioni", "Quindiciliardi",
                "Sedicilioni", "Sediciliardi", "Diciasettilioni"
            ]
        },
        new()
        {
            Group = DigitGroup.Thousands,
            Language = Language.Turkish,
            Names = (List<string>)
            [
                string.Empty, "Bin", "Milyon", "Milyar", "Trilyon", "Katrilyon", " Quadrillion", " Quintillion",
                " Sextillian", " Septillion", " Octillion", " Nonillion", " Decillion", " Undecillion",
                " Duodecillion", " Tredecillion", " Quattuordecillion", " Quindecillion", " Sexdecillion",
                " Septendecillion", " Octodecillion", " Novemdecillion", " Vigintillion", " Unvigintillion",
                " Duovigintillion", " 10^72", " 10^75", " 10^78", " 10^81", " 10^84", " 10^87", " Vigintinonillion",
                " 10^93", " 10^96", " Duotrigintillion", " Trestrigintillion"
            ]
        },
        new()
        {
            Group = DigitGroup.Thousands,
            Language = Language.Arabic,
            Names = (List<string>)
            [
                string.Empty, " الف", " مليون", " مليار", " بليون", " بليار", " ترليون", " تريليار", " كريليون",
                " كزيليار", " سنكليون", " سنكليار", " سيزيليون", " سيزيليار", " سيتليون", " سيتليار", " ويتليون",
                " ويتليار", " تيفليون", " تيفليار", " ديشليون", " ديشليار", " Unvigintillion", " Duovigintillion",
                " 10^72", " 10^75", " 10^78", " 10^81", " 10^84", " 10^87", " Vigintinonillion", " 10^93", " 10^96",
                " Duotrigintillion", " Trestrigintillion"
            ]
        },
        new()
        {
            Group = DigitGroup.Thousands,
            Language = Language.Russian,
            Names = (List<string>)
            [
                string.Empty, " Тысяча", " Миллион", " Миллиард", " Триллион", " Квадриллион", " Квинтиллион",
                " Секстиллиан", " Септиллион", " Октиллион", " Нониллион", " Дециллион", " Ундециллион",
                " Дуодециллион", " Тредециллион", " Кваттуордециллион", " Квиндециллион", " Сексдециллион",
                " Септдециллион", " Октодециллион", " Новемдециллион", " Вигинтиллион", " Унвигинтиллион",
                " Дуовигинтиллион", " 10 ^ 72", " 10 ^ 75", " 10 ^ 78", " 10 ^ 81", " 10 ^ 84", " 10 ^ 87",
                " Вигинтинониллион", " 10 ^ 93", " 10 ^ 96", " Дуотригинтиллион", " Трестригинтиллион"
            ]
        },
        new()
        {
            Group = DigitGroup.Thousands,
            Language = Language.Chinese,
            Names = (List<string>)
            [
                string.Empty, "千", "百万", "十亿", "万亿", "千万亿", "百兆", "十万兆", "京", "千京", "百万京", "十垓", "万垓", "千万垓", "百秭",
                "十万秭", "穰", "千穰", "百万穰", "十沟", "万沟", "千万沟", " 百涧", "十万涧", "正", "千正", "百万正", "十载", "万载", "千万载", "百极",
                "十万极", "恒河沙", "千恒河沙", "百万恒河沙"
            ]
        },
        new()
        {
            Group = DigitGroup.Thousands,
            Language = Language.Spanish,
            Names = (List<string>)
            [
                string.Empty, " Mil", " Millón", " Billón", " Trillón", " Cuatrillón", " Quintrillón", " Sextillón",
                " Septillón", " Octrillón", " Nonillón", " Decillión", " Undecillón", " Duodecillón", " 10^72",
                " 10^75", " 10^78", " 10^81", " 10^84", " 10^87", " Vigintillón", " 10^93", " 10^96",
                " Duotrigintillón", " Trestrigintillón"
            ]
        },
        new()
        {
            Group = DigitGroup.Thousands,
            Language = Language.Portuguese,
            Names = (List<string>)
            [
                string.Empty, " Mil", " Milhão", "  Bilhão", " Trilhão", " Quatrilhão", " Quintilhão", " Sextilhão",
                " Septilhão", " Octilhão", " Nonilhão", " Decilhão", " Undecilhão", " Dudecilhão ", " Tredecilhão",
                " Quadriodecilhão", " Quindecilhão", " ", " Seisdecilhão", " Oitodecillhão", " Novedecilhão",
                " Vigintilhão", " Unvigintilhão", " Duovigintilhão", " 10^72", " 10^75", " 10^78", " 10^81",
                " 10^84", " 10^87", " Vigintinonilhão", " 10^93", " 10^96", " Duotrigintilhão", " Trestrigintilhão"
            ]
        },
        new()
        {
            Group = DigitGroup.Thousands,
            Language = Language.French,
            Names = (List<string>)
            [
                string.Empty, " Mille", " Million", " Milliard", " Billion", " Billiard", " Trillion", " Trilliard",
                " Quadrillion", " Quadrilliard", " Quintillion", " Quintilliard", " Sextillion", " Sextilliard",
                " Septillion", " Septilliard", " Octillion", " Octilliard", " Nonillion", " Nonilliard",
                " Decillion", " Decilliard", " Undecillion", " Undecilliard", " Duodecillion", " Duodecilliard",
                " Tredecillion", " Tredecilliard", " Quattuordecillion", " Quattuordecilliard", " Quindecillion",
                " Quindecilliard", " Sexdecillion", " Sexdecilliard", " Septendecillion"
            ]
        },
        new()
        {
            Group = DigitGroup.Thousands,
            Language = Language.German,
            Names = (List<string>)
            [
                string.Empty, " Tausend", " Million", " Milliarde", " Billion", " Billiard", " Trillion",
                " Trilliarde", " Quadrillion", " Quadrilliarde", " Quintillion", " Quintilliarde", " Sextillion",
                " Sextilliarde", " Septillion", " Septilliarde", " Octillion", " Octilliarde", " Nonillion",
                " Nonilliarde", " Decillion", " Decilliarde", " Undecillion", " Undecilliarde", " Duodecillion",
                " Duodecilliarde", " Tredecillion", " Tredecilliarde", " Quattuordecillion", " Quattuordecilliarde",
                " Quindecillion", " Quindecilliarde", " Sexdecillion", " Sexdecilliarde", " Septendecillion"
            ]
        },
        new()
        {
            Group = DigitGroup.Thousands,
            Language = Language.Dutch,
            Names = (List<string>)
            [
                string.Empty, "Duizend", "Miljoen", "Miljard", "Biljoen", "Biljard", "Triljoen", "Triljard",
                "Quadriljoen", "Quadriljard", "Quintiljoen", "Quintiljard", "Sextiljoen", "Sextiljard",
                "Septiljoen", "Septiljard", "Octiljoen", "Octiljard", "Noniljoen", "Noniljard", "Deciljoen",
                "Deciljard", "Undeciljoen", "Undeciljard", "Duodeciljoen", "Duodeciljard", "Tredeciljoen",
                "Tredeciljard", "Quattuordeciljoen", "Quattuordeciljard", "Quindeciljoen", "Quindeciljard",
                "Sexdeciljoen", "Sexdeciljard", "Septendeciljoen"
            ]
        },
        new()
        {
            Group = DigitGroup.Thousands,
            Language = Language.Azerbaijani,
            Names = (List<string>)
            [
                string.Empty, " Min", " milyon", " Milyard", " Trilyon", " Kvadrilyon", " Kvintilyon",
                " Sekstillian", " Septilyon", " Oktilyon", " Nonillion", " Decillion", " Undecillion",
                " Duodecillion", " Tredecillion", " Quattuordecillion", " Quindecillion", " Sexdecillion",
                " Septendecillion", " Octodecillion", " Novemdecillion", " Vigintillion", " Unvigintillion",
                " Duovigintillion", " 10^72", " 10^75", " 10^78", " 10^81", " 10^84", " 10^87", " Vigintinonillion",
                " 10^93", " 10^96", " Duotrigintillion", " Trestrigintillion"
            ]
        }
    ];

    private readonly Dictionary<Language, string> _zero = new()
    {
        {
            Language.English, "Zero"
        },
        {
            Language.Persian, "صفر"
        },
        {
            Language.Norwegian, "Null"
        },
        {
            Language.Italian, "Zero"
        },
        {
            Language.Turkish, "Sıfır"
        },
        {
            Language.Arabic, "صفر"
        },
        {
            Language.Russian, "Ноль"
        },
        {
            Language.Chinese, "零"
        },
        {
            Language.Spanish, "Cero"
        },
        {
            Language.Portuguese, "Zero"
        },
        {
            Language.French, "Zéro"
        },
        {
            Language.German, "Null"
        },
        {
            Language.Dutch, "Nul"
        },
        {
            Language.Azerbaijani, "Sıfır"
        }
    };

    /// <summary>
    ///     display a numeric value using the equivalent text
    /// </summary>
    /// <param name="number">input number</param>
    /// <param name="language">local language</param>
    /// <returns>the equivalent text</returns>
    public string GetText(int number, Language language) => NumberToText((long)number, language);

    /// <summary>
    ///     display a numeric value using the equivalent text
    /// </summary>
    /// <param name="number">input number</param>
    /// <param name="language">local language</param>
    /// <returns>the equivalent text</returns>
    public string GetText(long number, Language language) => NumberToText(number, language);

    /// <summary>
    ///     display a numeric value using the equivalent text
    /// </summary>
    /// <param name="number">input number</param>
    /// <param name="language">local language</param>
    /// <returns>the equivalent text</returns>
    public string NumberToText(int number, Language language) => NumberToText((long)number, language);

    /// <summary>
    ///     display a numeric value using the equivalent text
    /// </summary>
    /// <param name="number">input number</param>
    /// <param name="language">local language</param>
    /// <returns>the equivalent text</returns>
    public string NumberToText(uint number, Language language) => NumberToText((long)number, language);

    /// <summary>
    ///     display a numeric value using the equivalent text
    /// </summary>
    /// <param name="number">input number</param>
    /// <param name="language">local language</param>
    /// <returns>the equivalent text</returns>
    public string NumberToText(byte number, Language language) => NumberToText((long)number, language);

    /// <summary>
    ///     display a numeric value using the equivalent text
    /// </summary>
    /// <param name="number">input number</param>
    /// <param name="language">local language</param>
    /// <returns>the equivalent text</returns>
    public string NumberToText(decimal number, Language language) => NumberToText((long)number, language);

    /// <summary>
    ///     display a numeric value using the equivalent text
    /// </summary>
    /// <param name="number">input number</param>
    /// <param name="language">local language</param>
    /// <returns>the equivalent text</returns>
    public string NumberToText(double number, Language language) => NumberToText((long)number, language);

    /// <summary>
    ///     display a numeric value using the equivalent text
    /// </summary>
    /// <param name="number">input number</param>
    /// <param name="language">local language</param>
    /// <returns>the equivalent text</returns>
    public string NumberToText(long number, Language language)
    {
        if (number == 0)
        {
            return _zero[language];
        }

        if (number < 0)
        {
            return _negative[language] + NumberToText(-number, language);
        }

        return Wordify(number, language, string.Empty, thousands: 0);
    }

    private string GetName(int idx, Language language, DigitGroup group)
        => _numberWords.First(x => x.Group == group && x.Language == language).Names[idx];

    private string Wordify(long number, Language language, string leftDigitsText, int thousands)
    {
        if (number == 0)
        {
            return leftDigitsText;
        }

        var wordValue = leftDigitsText;

        if (wordValue.Length > 0)
        {
            wordValue += _and[language];
        }

        if (number < 10)
        {
            wordValue += GetName((int)number, language, DigitGroup.Ones);
        }
        else if (number < 20)
        {
            wordValue += GetName((int)(number - 10), language, DigitGroup.Teens);
        }
        else if (number < 100)
        {
            wordValue += Wordify(number % 10, language, GetName((int)(number / 10 - 2), language, DigitGroup.Tens),
                thousands: 0);
        }
        else if (number < 1000)
        {
            wordValue += Wordify(number % 100, language, GetName((int)(number / 100), language, DigitGroup.Hundreds),
                thousands: 0);
        }
        else
        {
            wordValue += Wordify(number % 1000, language, Wordify(number / 1000, language, string.Empty, thousands + 1),
                thousands: 0);
        }

        if (number % 1000 == 0)
        {
            return wordValue;
        }

        return wordValue + GetName(thousands, language, DigitGroup.Thousands);
    }
}