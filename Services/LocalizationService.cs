using DanialNetAccount.Models;

namespace DanialNetAccount.Services
{
    public class LocalizationService
    {
        private readonly GlobalSetting _settings;

        public LocalizationService(GlobalSetting settings)
        {
            _settings = settings;
        }

        public string Translate(string key)
        {
            if (_settings.Language == AppLanguage.English) return key;

            return key switch
            {
                "Dashboard" => "داشبورد",
                "Accounts" => "حساب‌ها",
                "Journal" => "دفتر روزنامه",
                "Invoices" => "فاکتورها",
                "Reports" => "گزارشات",
                "Settings" => "تنظیمات",
                "Total Sales" => "کل فروش",
                "Inventory Value" => "ارزش انبار",
                "Outstanding AP" => "بدهی پرداختنی",
                "Net Profit" => "سود خالص",
                "Quick Actions" => "دسترسی سریع",
                "New Journal Entry" => "ثبت سند جدید",
                "Create Invoice" => "صدور فاکتور",
                "Financial Reports" => "گزارشات مالی",
                "Chart of Accounts" => "کدینگ حساب‌ها",
                "Create New Account" => "ایجاد حساب جدید",
                "General Journal" => "دفتر روزنامه عمومی",
                "New Entry" => "سند جدید",
                "Date" => "تاریخ",
                "Description" => "شرح",
                "Debit" => "بدهکار",
                "Credit" => "بستانکار",
                "Account" => "حساب",
                "Total" => "جمع کل",
                "Post Entry" => "ثبت سند",
                "Number" => "شماره",
                "Customer" => "مشتری",
                "Amount" => "مبلغ",
                "Status" => "وضعیت",
                "Active" => "فعال",
                "Voided" => "ابطال شده",
                "View" => "مشاهده",
                "New Invoice" => "فاکتور جدید",
                "Product / Service" => "کالا / خدمات",
                "Qty" => "تعداد",
                "Unit Price" => "فی",
                "Line Total" => "جمع ردیف",
                "Subtotal" => "جمع جزء",
                "VAT (9%)" => "مالیات (۹٪)",
                "Create & Post Invoice" => "صدور و ثبت فاکتور",
                "Official Invoice" => "فاکتور رسمی",
                "Print" => "چاپ",
                "Download PDF" => "دانلود PDF",
                "Void Invoice" => "ابطال فاکتور",
                "Trial Balance" => "تراز آزمایشی",
                "Profit & Loss" => "سود و زیان",
                "Balance Sheet" => "ترازنامه",
                "Recalculate" => "محاسبه مجدد",
                "Revenue" => "درآمدها",
                "Expenses" => "هزینه‌ها",
                "Bottom Line" => "نتیجه نهایی",
                "Profit Achieved" => "سود دهی",
                "Loss Incurred" => "زیان دهی",
                "Assets" => "دارایی‌ها",
                "Liabilities" => "بدهی‌ها",
                "Equity" => "سرمایه",
                "System Configuration" => "تنظیمات سیستم",
                "Company Name" => "نام شرکت",
                "Business Type" => "نوع کسب و کار",
                "Valuation Method" => "روش قیمت‌گذاری",
                "Lock Period (Until)" => "قفل تراکنش‌ها (تا تاریخ)",
                "Apply Settings" => "ذخیره تنظیمات",
                "Language" => "زبان",
                "Currency" => "واحد پول",
                _ => key
            };
        }

        public string FormatCurrency(decimal amount)
        {
            if (_settings.Currency == CurrencyType.USD)
                return amount.ToString("C2");

            return amount.ToString("N0") + " تومان";
        }

        public string GetDirection() => _settings.Language == AppLanguage.Persian ? "rtl" : "ltr";
    }
}
