namespace Travel.Core.Exceptions
{
    public class ErrorConstants
    {

        // General
        public const string NULL_ENTRY_REJECTED = "LI001";
        public const string AUTHORIZATION_ERROR = "LI002";

        public const string USER_ACCOUNT_INVALID_OTP = "USR007";
        public const string ROLE_NOT_EXIST = "ROL001";
        public const string USER_ACCOUNT_EXISTS = "USR001";
        public const string USER_ACCOUNT_NOT_EXIST = "USR002";
        public const string USER_ACCOUNT_LOCKED = "USR005";
        public const string USER_ACCOUNT_WRONG_OTP = "USR006";
        public const string USER_ACCOUNT_REGISTRATION_FAILED = "USR003";
        public const string USER_ACCOUNT_PASSWORD_INVALID = "USR004";

        // AssignedMenuToRole
        public const string ASSIGNEMENUROLE_EXIST = "AMR001";
        public const string ASSIGNEMENUROLE_NOT_EXIST = "AMR002";
        public const string DELETEPARENTMENU_EXIST_SUBMENU = "AMR003";
        public const string MENU_ALREADY_ASSIGNEDTO_ROLE = "AMR004";

        // Partners
        // TODO:
        public const string INSUFFICIENT_BALANCE = "PRT005";
        public const string REPAIR_LIMIT_EXCEEDED = "PRT006";

        // Routes
        public const string ROUTE_EXIST = "RUT001";
        public const string ROUTE_NOT_EXIST = "RUT002";

        // EntityCounts
        public const string ENTITY_COUNT_EXIST = "ENTC001";
        public const string ENTITY_COUNT_NOT_EXIST = "ENTC002";

        // Captains
        public const string DRIVER_EXIST = "DRV001";
        public const string DRIVER_NOT_EXIST = "DRV002";

        // Vehicle Makes
        public const string VEHICLE_MAKE_EXIST = "VMK001";
        public const string VEHICLE_MAKE_NOT_EXIST = "VMK002";

        // Terminal
        public const string TERMINAL_EXIST = "TRM001";
        public const string TERMINAL_NOT_EXIST = "TRM002";

        // PassengerType
        public const string PASSENGERTYPE_EXIST = "PSGT001";
        public const string PASSENGERTYPE_NOT_EXIST = "PSGT002";

        // AccountType
        public const string ACCOUNT_TYPE_EXIST = "ACT001";
        public const string ACCOUNT_TYPE_NOT_EXIST = "ACT002";

        // CalendarManagement
        public const string CALENDER_MANAGEMENT_EXIST = "CMT001";
        public const string CALENDAR_MANAGEMENT_NOT_EXIST = "CMT002";

        // Employee
        public const string EMPLOYEE_EXIST = "EMP001";
        public const string EMPLOYEE_NOT_EXIST = "EMP002";

        // VehicleModel
        public const string VEHICLE_MODEL_EXIST = "VMD001";
        public const string VEHICLE_MODEL_NOT_EXIST = "VMD002";

        // Vehicle
        public const string VEHICLE_EXIST = "VCL001";
        public const string VEHICLE_NOT_EXIST = "VCL002";

        // VehicleParts
        public const string VEHICLE_PARTS_EXIST = "VPT001";
        public const string VEHICLE_PARTS_NOT_EXIST = "VPT002";

        //VehicleMaintenamcePartTypes
        public const string VEHICLE_PARTS_MAINTENANCE_TYPE_EXIST = "VPMT001";
        public const string VEHICLE_PARTS_MAINTENANCE_TYPE_NOT_EXIST = "VPMT002";

        // VehiclePartMaintenanceCheck
        public const string VEHICLE_PARTS_MAINTENANCE_EXIST = "VPTM001";
        public const string VEHICLE_PARTS_MAINTENANCE_NOT_EXIST = "VPTM002";

        // Vendor
        public const string VENDR_EXIST = "VED001";
        public const string VENDOR_NOT_EXIST = "VED002";

        // Vendor
        public const string VEHICLE_STATUS_EXIST = "VSTA001";
        public const string VEHICLE_STATUS_NOT_EXIST = "VSTA002";


        // Fare
        public const string FARE_EXIST = "FAR001";
        public const string FARE_NOT_EXIST = "FAR002";

        // Position
        public const string POSITION_EXIST = "POS001";
        public const string POSITION_NOT_EXIST = "POS002";

        // Department
        public const string DEPARTMENT_EXIST = "DEPT001";
        public const string DEPARTMENT_NOT_EXIST = "DEPT002";

        // LoyaltyFeature
        public const string LOYALTY_FEATURE_EXIST = "LOYF001";
        public const string LOYALTY_FEATURE_NOT_EXIST = "LOYF002";

        // LoyaltySettings
        public const string LOYALTY_SETTINGS_EXIST = "LOYS001";
        public const string LOYALTY_SETTINGS_NOT_EXIST = "LOYS002";


        // MaintenanceCategory
        public const string MAINTENANCE_CATEGORY_EXIST = "MATC001";
        public const string MAINTENANCE_CATEGORY_NOT_EXIST = "MATC002";

        // MaintenancePartChart
        public const string MAINTENANCE_PART_CHART_EXIST = "MATPC001";
        public const string MAINTENANCE_PART_CHART_NOT_EXIST = "MATPC002";

        // Region
        public const string REGION_EXIST = "REGN001";
        public const string REGION_NOT_EXIST = "REGN002";

        // State
        public const string STATE_EXIST = "STAT001";
        public const string STATE_NOT_EXIST = "STAT002";

        // VehiclPartInventory
        public const string VEHICLE_PART_INEVNTORY_EXIST = "VPI001";
        public const string VEHICLE_PART_INEVNTORY_NOT_EXIST = "VPI002";

        // Store
        public const string STORE_EXIST = "STOR001";
        public const string STORE_NOT_EXIST = "STOR002";

        // Trip
        public const string TRIP_EXIST = "TRIP001";
        public const string TRIP_NOT_EXIST = "TRIP002";
        public const string TRIP_NO_AVAILABLE_BUSES = "TRIP003";
        public const string VEHICLETRIP_EXIST = "VEHTRIP001";
        public const string VEHICLETRIP_NOT_EXIST = "VEHTRIP002";

        // VendorType
        public const string VENDOR_TYPE_EXIST = "VENT001";
        public const string VENDOR_TYPE_NOT_EXIST = "VENT002";

        // WalletNumber
        public const string WALLET_NUMBER_EXIST = "WATN001";
        public const string WALLET_NUMBER_NOT_EXIST = "WATN002";

        // Wallet
        public const string WALLET_EXIST = "WAL001";
        public const string WALLET_NOT_EXIST = "WAL002";

        //Wallet
        public const string WALLET_NUMBER_EXHAUSTED = "WALT001";

        //PointupPoint
        public const string PICKUPPOINT_EXIST = "PIKP001";
        public const string PICKUPPOINT_NOT_EXIST = "PIKP002";

        //--------- Booking --start
        public const string BOOKING_EXISTS = "BOOK001";
        public const string BOOKING_NOT_EXIST = "BOOK002";
        public const string BOOKING_INVALID_RETURN_DATE = "BOOK003";

        // BusTripRegistration
        public const string BUSTRIP_REGISTRATION_EXISTS = "BUSTRIP001";
        public const string BUSTRIP_REGISTRATION_NOT_EXIST = "BUSTRIP002";

        // HiredBooking
        public const string HIRED_BOOKING_EXISTS = "HRBOOK001";
        public const string HIRED_BOOKING_NOT_EXIST = "HRBOOK002";

        // JourneyManagement
        public const string JOURNEY_MANAGEMENT_EXISTS = "JNYMGT001";
        public const string JOURNEY_MANAGEMENT_NOT_EXIST = "JNYMGT002";
        public const string JOURNEY_COULD_NOT_UPDATE = "JNYMGT003";
        public const string JOURNEY_NOT_APPROVED = "JNYMGT004";
        public const string JOURNEY_STILL_ACCEPTED_BY_CAPTAIN = "JNYMGT005";
        public const string JOURNEY_NOT_ACCEPTED_BY_CAPTAIN = "JNYMGT006";
        public const string JOURNEY_NOT_ATTACHED_TO_CAPTAIN = "JNYMGT007";


        // ManifestManagement
        public const string MANIFEST_MANAGEMENT_EXISTS = "MFSTMGT001";
        public const string MANIFEST_MANAGEMENT_NOT_EXIST = "MFSTMGT002";

        // SeatManagement
        public const string SEAT_MANAGEMENT_EXISTS = "SEATMGT001";
        public const string SEAT_MANAGEMENT_NOT_EXIST = "SEATMGT002";

        // Discount
        public const string DISCOUNT_EXISTS = "DISC001";
        public const string DISCOUNT_NOT_EXIST = "DISC002";

        // FareCalendar
        public const string FareCalendar_EXISTS = "FACL001";
        public const string FareCalendar_NOT_EXIST = "FACL002";

        // Coupon
        public const string COUPON_EXISTS = "COUPON001";
        public const string COUPON_NOT_EXIST = "COUPON002";


        //---------- Booking --end

        //StockRequest
        public const string STOCK_REQUEST_EXIST = "STR001";
        public const string STOCK_REQUEST_NOT_EXIST = "STR002";


        //StockRequestPart
        public const string STOCK_REQUEST_PART_EXIST = "STRP001";
        public const string STOCK_REQUEST_PART_NOT_EXIST = "STRP002";

        //StockRequestStatus
        public const string STOCK_REQUEST_STATUS_EXIST = "STRS001";
        public const string STOCK_REQUEST_STATUS_NOT_EXIST = "STRS002";

        //JobCardStatus
        public const string JOB_CARD_STATUS_EXIST = "JBCS001";
        public const string JOB_CARD_STATUS_NOT_EXIST = "JBCS002";

        //JobCardMaintenanceStatus
        public const string JOB_CARD_MAINTENANCE_STATUS_EXIST = "JBMS001";
        public const string JOB_CARD_MAINTENANCE_STATUS_NOT_EXIST = "JBMS002";

        //Workshop
        public const string WORKSHOP_EXIST = "WSP001";
        public const string WORKSHOP_NOT_EXIST = "WSP002";

        //RequestType
        public const string REPAIR_TYPE_EXIST = "REPT001";
        public const string REPAIR_TYPE_NOT_EXIST = "REPT002";

        //JobCardManagement
        public const string JOBCARD_MANAGEMENT_EXIST = "JCMP001";
        public const string JOBCARD_MANAGEMENT_NOT_EXIST = "JCMP002";

        //JobCardManagementPart
        public const string JOBCARD_MANAGEMENT_PART_EXIST = "JCMP001";
        public const string JOBCARD_MANAGEMENT_PART_NOT_EXIST = "JCMP002";

        //JobCard
        public const string JOBCARD_EXIST = "JBC001";
        public const string JOBCARD_NOT_EXIST = "JBC002";
        public const string JOBCARD_NOT_IN_A_VALID_STATE = "JBC003";


        //FeeType
        public const string FEETYPE_EXIST = "FEETY001";
        public const string FEETYPE_NOT_EXIST = "FEETY002";

        //Customer
        public const string CUSTOMER_EXIST = "CUSTM001";
        public const string CUSTOMER_NOT_EXIST = "CUSTM002";

        //Feedback
        public const string FEEDBACK_EXIST = "STR001";
        public const string FEEDBACK_NOT_EXIST = "STR002";

        //Referral
        public const string INVALID_REFERRAL_CODE = "Invalid referral code, pls use a valid one or leave the field empty";

        //Franchise 
        public const string FRANCHISE_EXIST = "FRCH001";
        public const string FRANCHISE_NOT_EXIST = "FRCH002";

    }
}
