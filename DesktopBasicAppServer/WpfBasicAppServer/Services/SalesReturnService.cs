using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows;
using WpfAccountServerApp.General;

namespace WpfAccountServerApp.Services
{   
    public class SalesReturnService : ISalesReturn
    {
        private string mBillType = "SR";

        public bool CreateBill(CSalesReturn oSalesReturn)
        {
            bool returnValue = false;

            lock (Synchronizer.@lock)
            {
                
                using (var dataB = new Database9001Entities())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {

                        ProductService ls = new ProductService();
                        BillNoService bs = new BillNoService();


                        int cbillNo = bs.ReadNextSalesReturnBillNo(oSalesReturn.FinancialCode);
                        bs.UpdateSalesReturnBillNo(oSalesReturn.FinancialCode,cbillNo+1);

                        List<string> barcodes = new BarcodeService().ReadBarcodes(oSalesReturn.Details.Count);
                        
                        for (int i = 0; i < oSalesReturn.Details.Count; i++)
                        {
                            product_transactions pt = new product_transactions();

                            pt.bill_no= cbillNo.ToString();
                            pt.bill_type = mBillType;                            
                            pt.bill_date_time = oSalesReturn.BillDateTime;
                            pt.ref_bill_no = oSalesReturn.RefBillNo;
                            pt.ref_bill_date_time = oSalesReturn.RefBillDateTime;
                            pt.customer_code = oSalesReturn.CustomerCode;
                            pt.customer = oSalesReturn.Customer;
                            pt.customer_address = oSalesReturn.CustomerAddress;
                            pt.narration = oSalesReturn.Narration;
                            pt.advance = oSalesReturn.Advance;
                            pt.extra_charges = oSalesReturn.Expense;
                            pt.discounts = oSalesReturn.Discount;
                            pt.financial_code = oSalesReturn.FinancialCode;

                            pt.serial_no = oSalesReturn.Details.ElementAt(i).SerialNo;
                            pt.product_code = oSalesReturn.Details.ElementAt(i).ProductCode;
                            pt.product = oSalesReturn.Details.ElementAt(i).Product;
                            pt.sales_unit = oSalesReturn.Details.ElementAt(i).SalesReturnUnit;
                            pt.sales_unit_code = oSalesReturn.Details.ElementAt(i).SalesReturnUnitCode;
                            pt.sales_unit_value = oSalesReturn.Details.ElementAt(i).SalesReturnUnitValue;
                            pt.quantity = oSalesReturn.Details.ElementAt(i).Quantity*-1;
                            pt.sales_rate = oSalesReturn.Details.ElementAt(i).SalesReturnRate;
                            pt.mrp = oSalesReturn.Details.ElementAt(i).MRP;
                            //get a barcode here
                            pt.barcode = barcodes.ElementAt(i);
                            pt.unit_code = oSalesReturn.Details.ElementAt(i).SalesReturnUnitCode;
                            pt.unit_value = oSalesReturn.Details.ElementAt(i).SalesReturnUnitValue;

                            dataB.product_transactions.Add(pt);                            
                        }

                        dataB.SaveChanges();
                        //Success
                        returnValue = true;

                        dataBTransaction.Commit();
                    }
                    catch(Exception e)
                    {                        
                        dataBTransaction.Rollback();
                    }
                    finally
                    {

                    }
                }                
            }

            return returnValue;
        }

        public bool DeleteBill(string billNo,string financialCode)
        {
            bool returnValue = true;

            lock (Synchronizer.@lock)
            {
                using (var dataB = new Database9001Entities())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {
                        //Delete the transaction
                        var cpp = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == billNo && x.financial_code == financialCode && x.bill_type == mBillType);
                        dataB.product_transactions.RemoveRange(cpp);
                        
                        dataB.SaveChanges();                        
                        dataBTransaction.Commit();
                    }
                    catch
                    {
                        returnValue = false;
                        dataBTransaction.Rollback();
                    }
                    finally
                    {

                    }
                }
            }
            return returnValue;
        }

        public CSalesReturn ReadBill(string billNo,string financialCode)
        {
            CSalesReturn ccp = null;

            using (var dataB = new Database9001Entities())
            {
                var cps = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == billNo && x.financial_code == financialCode&&x.bill_type==mBillType).OrderBy(y=>y.serial_no);
                
                if (cps.Count() > 0)
                {
                    ccp = new CSalesReturn();

                    var cp = cps.FirstOrDefault();
                    ccp.Id = cp.id;
                    ccp.BillNo = cp.bill_no;
                    ccp.BillDateTime = cp.bill_date_time;
                    ccp.RefBillNo = cp.ref_bill_no;
                    ccp.RefBillDateTime = (DateTime)cp.ref_bill_date_time;
                    ccp.CustomerCode = cp.customer_code;
                    ccp.Customer = cp.customer;
                    ccp.CustomerAddress = cp.customer_address;
                    ccp.Narration = cp.narration;
                    ccp.Advance = (decimal)cp.advance;
                    ccp.Expense = (decimal)cp.extra_charges;
                    ccp.Discount = (decimal)cp.discounts;
                    ccp.FinancialCode = cp.financial_code;
                    
                    foreach (var item in cps)
                    {                        
                        ccp.Details.Add(new CSalesReturnDetails() { SerialNo = (int)item.serial_no, ProductCode = item.product_code, Product = item.product, SalesReturnUnit = item.sales_unit, SalesReturnUnitCode = item.sales_unit_code, SalesReturnUnitValue = (decimal)item.sales_unit_value, Quantity = (decimal)item.quantity*-1, SalesReturnRate = (decimal)item.sales_rate, MRP = (decimal)item.mrp, Total = (decimal)(item.quantity * item.sales_rate*-1), Barcode = item.barcode, OldQuantity= (decimal)item.quantity*-1 });                        
                    }
                }
                
            }

            return ccp;
        }

        public CSalesReturn ReadSalesBill(string billNo, string financialCode)
        {
            CSalesReturn ccp = null;

            using (var dataB = new Database9001Entities())
            {
                var cps = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == billNo && x.financial_code == financialCode && x.bill_type == "S").OrderBy(y => y.serial_no);

                if (cps.Count() > 0)
                {
                    ccp = new CSalesReturn();

                    var cp = cps.FirstOrDefault();
                    ccp.Id = cp.id;
                    ccp.BillNo = cp.bill_no;
                    ccp.BillDateTime = cp.bill_date_time;
                    ccp.CustomerCode = cp.customer_code;
                    ccp.Customer = cp.customer;
                    ccp.CustomerAddress = cp.customer_address;
                    ccp.Narration = cp.narration;
                    ccp.Advance = (decimal)cp.advance;
                    ccp.Expense = (decimal)cp.extra_charges;
                    ccp.Discount = (decimal)cp.discounts;
                    ccp.FinancialCode = cp.financial_code;

                    int serialNo = 1;
                    foreach (var item in cps)
                    {
                        ccp.Details.Add(new CSalesReturnDetails() { SerialNo = serialNo++, ProductCode = item.product_code, Product = item.product, SalesReturnUnit = item.sales_unit, SalesReturnUnitCode = item.sales_unit_code, SalesReturnUnitValue = (decimal)item.sales_unit_value, Quantity = (decimal)item.quantity*-1, SalesReturnRate = (decimal)item.sales_rate, MRP = (decimal)item.mrp, Total = (decimal)(item.quantity * item.sales_rate*-1), Barcode = item.barcode, OldQuantity = (decimal)item.quantity*-1 });
                    }
                }

            }

            return ccp;
        }

        public int ReadNextBillNo(string financialCode)
        {
            
            BillNoService bns = new BillNoService();
            return bns.ReadNextSalesReturnBillNo(financialCode);
            
        }

        public bool UpdateBill(CSalesReturn oSalesReturn)
        {
            bool returnValue = false;

            lock (Synchronizer.@lock)
            {
                using (var dataB = new Database9001Entities())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {
                        var cpp = dataB.product_transactions.Select(c => c).Where(x => x.bill_no == oSalesReturn.BillNo&& x.financial_code==oSalesReturn.FinancialCode&&x.bill_type==mBillType);
                        dataB.product_transactions.RemoveRange(cpp);
                        
                        int serialNo = 1;
                        for (int i = 0; i < oSalesReturn.Details.Count; i++)
                        {

                            product_transactions pt = new product_transactions();

                            pt.bill_no = oSalesReturn.BillNo;
                            pt.bill_type = mBillType;
                            pt.bill_date_time = oSalesReturn.BillDateTime;
                            pt.ref_bill_no = oSalesReturn.RefBillNo;
                            pt.ref_bill_date_time = oSalesReturn.RefBillDateTime;
                            pt.customer_code = oSalesReturn.CustomerCode;
                            pt.customer = oSalesReturn.Customer;
                            pt.customer_address = oSalesReturn.CustomerAddress;
                            pt.narration = oSalesReturn.Narration;
                            pt.advance = oSalesReturn.Advance;
                            pt.extra_charges = oSalesReturn.Expense;
                            pt.discounts = oSalesReturn.Discount;
                            pt.financial_code = oSalesReturn.FinancialCode;

                            pt.serial_no = serialNo++;
                            pt.product_code = oSalesReturn.Details.ElementAt(i).ProductCode;
                            pt.product = oSalesReturn.Details.ElementAt(i).Product;
                            pt.sales_unit = oSalesReturn.Details.ElementAt(i).SalesReturnUnit;
                            pt.sales_unit_code = oSalesReturn.Details.ElementAt(i).SalesReturnUnitCode;
                            pt.sales_unit_value = oSalesReturn.Details.ElementAt(i).SalesReturnUnitValue;
                            pt.quantity = oSalesReturn.Details.ElementAt(i).Quantity * -1;
                            pt.sales_rate = oSalesReturn.Details.ElementAt(i).SalesReturnRate;
                            pt.mrp = oSalesReturn.Details.ElementAt(i).MRP;
                            pt.barcode = oSalesReturn.Details.ElementAt(i).Barcode;
                            pt.unit_code = oSalesReturn.Details.ElementAt(i).SalesReturnUnitCode;
                            pt.unit_value = oSalesReturn.Details.ElementAt(i).SalesReturnUnitValue;

                            dataB.product_transactions.Add(pt);

                        }
                        
                        dataB.SaveChanges();

                        //Success
                        returnValue = true;

                        dataBTransaction.Commit();
                    }
                    catch(Exception e)
                    {                        
                        dataBTransaction.Rollback();
                    }
                    finally
                    {
                    }
                }
            }
            return returnValue;
        }
        
    }
}
