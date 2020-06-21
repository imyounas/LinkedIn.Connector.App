using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIConnectScraperLib.Selectors
{
    public class LIConnectionRequestSelectors
    {
        public static string SearchPageConnButtonCSSSel = "div.search-result__actions > div > button.search-result__actions--primary.button-secondary-medium.m5";
        public static string SendConnectDivVisibleCSSSel = "div.send-invite__actions";
        public static string ProfileConnButtonCSSSel = "button.pv-s-profile-actions.pv-s-profile-actions--connect.button-primary-large.mh1";
        public static string ConnAddNoteButtonCSSSel = "div.send-invite__actions > button.button-secondary-large";
        public static string ConnNoteTextCSSSel = "textarea.send-invite__custom-message.mb3.ember-text-area";
        public static string ConnSendDoneButtonCSSSel = "div.send-invite__actions > button.button-primary-large";
    }

    public class LIDirectMessageSelectors
    {
        public static string SearchPageDirectMessageButtonCSSSel = "div.search-result__actions > div > div> button.message-anywhere-button.button-secondary-medium";
        public static string ProfileDirectMessageButtonCSSSel = "button.pv-s-profile-actions.pv-s-profile-actions--message.button-primary-large";
        public static string DirectMessageFormVisibleCSSSel = "form.msg-messaging-form__form > textarea.ember-text-area.msg-messaging-form__message";
        public static string DirectMessageTextCSSSel = "textarea.ember-text-area.msg-messaging-form__message";
        public static string DirectMessageSendButtonCSSSel = "footer.msg-messaging-form__form-footer > div.msg-messaging-form__right-actions > button.msg-messaging-form__send-button.button-tertiary-small";
    }
}
