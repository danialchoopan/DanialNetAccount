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
                "Revenue" => "درآمد",
                "Expenses" => "هزینه‌ها",
                "Total Revenue" => "جمع درآمدها",
                "Total Expenses" => "جمع هزینه‌ها",
                "Bottom Line" => "نتیجه نهایی",
                "Profit Achieved" => "سود دهی",
                "Loss Incurred" => "زیان دهی",
                "Assets" => "دارایی‌ها",
                "Liabilities" => "بدهی‌ها",
                "System Configuration" => "تنظیمات سیستم",
                "Company Name" => "نام شرکت",
                "Business Type" => "نوع کسب و کار",
                "Valuation Method" => "روش قیمت‌گذاری",
                "Lock Period (Until)" => "قفل تراکنش‌ها (تا تاریخ)",
                "Apply Settings" => "ذخیره تنظیمات",
                "Language" => "زبان",
                "Currency" => "واحد پول",
                "Purchase Invoice" => "فاکتور خرید",
                "All Types" => "همه انواع",
                "Sales Only" => "فقط فروش",
                "Purchases Only" => "فقط خرید",
                "Filter" => "فیلتر",
                "Type" => "نوع",
                "Ledger" => "دفتر معین",
                "Account Ledger" => "دفتر معین حساب",
                "From Date" => "از تاریخ",
                "To Date" => "تا تاریخ",
                "Filter Ledger" => "فیلتر دفتر",
                "Running Balance" => "مانده متحرک",
                "Closing Balance" => "مانده نهایی",
                "Sale" => "فروش",
                "Purchase" => "خرید",
                "Accounting Discrepancy" => "عدم تراز حسابداری",
                "The Balance Sheet is not balanced" => "ترازنامه تراز نیست",
                "Net profit for current period" => "سود خالص دوره جاری",
                "Search by number or customer" => "جستجو بر اساس شماره یا مشتری",
                "Customer Details" => "مشخصات مشتری",
                "Invoice Info" => "اطلاعات فاکتور",
                "Financial Performance" => "عملکرد مالی",
                "Budget Utilization" => "بهره‌وری بودجه",
                "Budget Note" => "شما ۷۶٪ از بودجه ماهانه پیش‌بینی شده را مصرف کرده‌اید.",
                "View Budget" => "مشاهده بودجه",
                "Valuation Hint" => "روش محاسبه بهای تمام شده کالای فروش رفته را تعیین می‌کند.",
                "Lock Hint" => "از تغییر تراکنش‌های قبل از این تاریخ جلوگیری می‌کند.",
                "Asset" => "دارایی",
                "Liability" => "بدهی",
                "Equity" => "سرمایه",
                "Expense" => "هزینه",
                "Invoice" => "فاکتور",
                "Add Row" => "افزودن ردیف",
                "Cancel" => "انصراف",
                "Entry purpose" => "شرح سند",
                "New Purchase Invoice" => "فاکتور خرید جدید",
                "Vendor Name" => "نام فروشنده",
                "Add Item" => "افزودن کالا",
                "DanialNet Account" => "دانیال‌نت اَکانت",
                "Enterprise Financial Management" => "مدیریت مالی پیشرفته",
                "Official Statement" => "گزارش رسمی",
                "Invoice Footer Note" => "از خرید شما سپاسگزاریم! مهلت پرداخت ۳۰ روز می‌باشد.",
                "Back to Invoices" => "بازگشت به لیست فاکتورها",
                "Void Confirmation" => "آیا از ابطال این فاکتور اطمینان دارید؟ تمام آثار مالی معکوس خواهد شد.",
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
