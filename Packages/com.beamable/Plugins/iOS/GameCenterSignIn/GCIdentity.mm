#import "GCIdentity.h"

//
//  Identity Callbacks
//

//  Must be in target GameObject. See example for more details
extern "C" {
    //  Call on success
    void OnIdentitySuccess(const char* gameObjectName, const char* identity)
    {
        UnitySendMessage(gameObjectName, "OnIdentitySuccess", identity);
    }
    
    //  Call on error
    void OnIdentityError(const char* gameObjectName, const char* error)
    {
        UnitySendMessage(gameObjectName, "OnIdentityError", error);
    }
}

@implementation GCIdentity

-(id)init
{
    self = [super init];
    return self;
}

//  Generate identity and invoke callback in Unity3D GameObject by his name
-(void) generateIdentity:(NSString*)gameObjectName {
    GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];
    
    //  Check player authentication
    if ( ! localPlayer.isAuthenticated) {
        OnIdentityError([gameObjectName UTF8String], "Player is not authenticated");
        return;
    }
    
    //  Start identity generator
    if (@available(iOS 13.5, *)) {
    [localPlayer fetchItemsForIdentityVerificationSignature:^(NSURL *publicKeyUrl, NSData *signature, NSData *salt, uint64_t timestamp, NSError *error) {
        if (error != nil) {
            OnIdentityError([gameObjectName UTF8String], [error.localizedDescription UTF8String]);
            return;
        }
        //  Build string with data split by ';' separator: <publicKeyUrl>;<signature>;<salt>;<timestamp>;
        NSString* identity = [NSString stringWithFormat:@"%@;%@;%@;%llu;%@", publicKeyUrl.absoluteString, [[NSString alloc] initWithData: [signature base64EncodedDataWithOptions:0] encoding: NSUTF8StringEncoding], [[NSString alloc] initWithData: [salt base64EncodedDataWithOptions:0] encoding: NSUTF8StringEncoding], timestamp, localPlayer.teamPlayerID];
        
        OnIdentitySuccess([gameObjectName UTF8String], [identity UTF8String]);
    }];
    } else {
        [localPlayer generateIdentityVerificationSignatureWithCompletionHandler:^(NSURL *publicKeyUrl, NSData *signature, NSData *salt, uint64_t timestamp, NSError *error) {
            if (error != nil) {
                OnIdentityError([gameObjectName UTF8String], [error.localizedDescription UTF8String]);
                return;
            }
            //  Build string with data split by ';' separator: <publicKeyUrl>;<signature>;<salt>;<timestamp>;
            NSString* identity = [NSString stringWithFormat:@"%@;%@;%@;%llu;%@", publicKeyUrl.absoluteString, [[NSString alloc] initWithData: [signature base64EncodedDataWithOptions:0] encoding: NSUTF8StringEncoding], [[NSString alloc] initWithData: [salt base64EncodedDataWithOptions:0] encoding: NSUTF8StringEncoding], timestamp, localPlayer.playerID];
            
            OnIdentitySuccess([gameObjectName UTF8String], [identity UTF8String]);
        }];
    }
}

@end

static GCIdentity* gcIdentity = nil;

//  Converts C style string to NSString
NSString* CreateNSString (const char* string)
{
    if (string)
        return [NSString stringWithUTF8String: string];
    else
        return [NSString stringWithUTF8String: ""];
}

//
//  Functions calls from Unity3D scripts
//

extern "C" {
    void _GenerateIdentity(const char* gameObjectName)
    {
        if (gcIdentity == nil)
            gcIdentity = [[GCIdentity alloc] init];
        
        [gcIdentity generateIdentity:CreateNSString(gameObjectName)];
    }
}
