#if __has_include(<GoogleSignIn/GIDSignIn.h>)

#import "BeamableGoogleSignIn.h"
#import <GoogleSignIn/GIDAuthentication.h>
#import <GoogleSignIn/GIDGoogleUser.h>
#import <GoogleSignIn/GIDSignIn.h>

#import <memory>


NSRecursiveLock *resultLock = [NSRecursiveLock alloc];
struct SignInCallback {
  NSString *callbackObject;
  NSString *callbackMethod;
};
std::unique_ptr<SignInCallback> callback_;

GoogleSignInHandler *gsiHandler;


@implementation GoogleSignInHandler

- (void)signIn:(GIDSignIn *)signIn
    didSignInForUser:(GIDGoogleUser *)user
    withError:(NSError *)_error {
  NSString *response = @"UNKNOWN";
  if (_error == nil) {
    NSLog(@"didSignInForUser: SUCCESS");
    response = [[user authentication] idToken];
  } else {
    NSLog(@"didSignInForUser: %@", _error.localizedDescription);
    response = [NSString stringWithFormat:@"EXCEPTION %@", _error.localizedDescription];
  }
  const char *callbackObject = [callback_->callbackObject UTF8String];
  const char *callbackMethod = [callback_->callbackMethod UTF8String];
  UnitySendMessage(callbackObject, callbackMethod, [response UTF8String]);
}

@end


extern "C" {
  void *GoogleSignIn_Login(const char *clientId, const char *callbackObject, const char *callbackMethod) {
    [resultLock lock];
    callback_.reset(new SignInCallback());
    callback_->callbackObject = [NSString stringWithUTF8String:callbackObject];
    callback_->callbackMethod = [NSString stringWithUTF8String:callbackMethod];
    [resultLock unlock];

    gsiHandler = [GoogleSignInHandler alloc];
    GIDSignIn *googleSignIn = [GIDSignIn sharedInstance];
    googleSignIn.clientID = [NSString stringWithUTF8String:clientId];
    googleSignIn.presentingViewController = UnityGetGLViewController();
    googleSignIn.delegate = gsiHandler;
    [googleSignIn signIn];
  }
}

#else

extern "C" {
  void *GoogleSignIn_Login(const char *clientId, const char *callbackObject, const char *callbackMethod) {
    UnitySendMessage(callbackObject, callbackMethod, [@"EXCEPTION Unable to find Google Sign-In framework" UTF8String]);
  }
}

#endif  // __has_include(<GoogleSignIn/GIDSignIn.h>)

